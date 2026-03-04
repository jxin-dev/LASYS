using System.Diagnostics;
using System.Drawing;
using LASYS.Application.Common.Enums;
using LASYS.Application.Events;
using LASYS.Application.Features.LabelProcessing.Abstractions;
using LASYS.Application.Features.LabelProcessing.Contracts;
using LASYS.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace LASYS.Application.Features.LabelProcessing.Services
{
    public class LabelProcessingService : ILabelProcessingService
    {

        private readonly ILogger<LabelProcessingService> _logger;
        private readonly ICameraService _cameraService;
        private readonly IOCRService _ocrService;
        private readonly ICalibrationService _calibrationService;
        private readonly IBarcodeService _barcodeService;
        private readonly IPrinterService _printerService;

        private readonly SemaphoreSlim _lock = new(1, 1);
        private const int MaxRetryPerStep = 3;
        private readonly List<OperatorDecisionLog> _decisionHistory = new();
        public event Action<LabelProcessingStatus>? StatusChanged;
        private void OnStatusChanged(LabelProcessingStatus status)
        {
            StatusChanged?.Invoke(status);
        }

        public LabelProcessingService(ILogger<LabelProcessingService> logger,
                                      ICameraService cameraService,
                                      IOCRService ocrService,
                                      ICalibrationService calibrationService,
                                      IBarcodeService barcodeService,
                                      IPrinterService printerService)
        {
            _logger = logger;
            _cameraService = cameraService;
            _ocrService = ocrService;
            _calibrationService = calibrationService;
            _barcodeService = barcodeService;
            _printerService = printerService;
        }

        public async Task StartJobAsync(Size viewerSize, StartLabelJobRequest request, CancellationToken token)
        {
            if (!await _lock.WaitAsync(0))
            {
                _logger.LogWarning("Attempted to start job while another job is running.");
                OnStatusChanged(LabelProcessingStatus.Busy);
                return; // Exit if already running
            }

            int printedCount = 0;
            int currentSequence = request.StartSequence;

            try
            {
                var calibrationResult = await _calibrationService.LoadAsync();
                if (calibrationResult.Products == null || calibrationResult.Products.Count == 0)
                {
                    _logger.LogWarning("No calibration data found.");
                    OnStatusChanged(LabelProcessingStatus.CalibrationNotFound);
                    return;
                }

                var product = calibrationResult.Products.FirstOrDefault(p => p.ItemCode == request.ItemCode);
                if (product == null)
                {
                    _logger.LogWarning("ItemCode {ItemCode} not found in calibration.", request.ItemCode);
                    OnStatusChanged(LabelProcessingStatus.ProductNotConfigured);
                    return;
                }

                var coordinates = product.Coordinates;

                while (printedCount < request.Quantity)
                {
                    token.ThrowIfCancellationRequested();
                    // Print label
                    OnStatusChanged(LabelProcessingStatus.Printing);
                    var labelData = new Dictionary<string, string>(request.LabelData);
                    labelData[request.SequenceVariableName] = currentSequence.ToString();

                    bool generated = _printerService.PrintLabelWithPreview($"LBL_{currentSequence}");
                    if (!generated)
                    {
                        OnStatusChanged(LabelProcessingStatus.Error);
                        return;
                    }
                    _printerService.Print();
                    await Task.Delay(150, token);

                    // Barcode Validation
                    int barcodeRetry = 0;
                    while (true)
                    {
                        token.ThrowIfCancellationRequested();

                        OnStatusChanged(LabelProcessingStatus.ScanningBarcode);

                        var scannedBarcode = await WaitForBarcodeAsync(token);
                        var expectedBarcode = labelData[request.BarcodeVariableName];

                        if (string.Equals(scannedBarcode?.Trim(), expectedBarcode.Trim(), StringComparison.Ordinal))
                        {
                            break; // barcode valid
                        }

                        barcodeRetry++;

                        _logger.LogWarning("Barcode mismatch. Attempt {Attempt}/{Max}. Expected {Expected}, Got {Actual}",
                            barcodeRetry, MaxRetryPerStep, expectedBarcode, scannedBarcode);

                        if (barcodeRetry >= MaxRetryPerStep)
                        {
                            _logger.LogError("Max barcode retries reached.");

                            OnStatusChanged(LabelProcessingStatus.WaitingForOperator);

                            var decision = await WaitForOperatorDecisionAsync(token);

                            _decisionHistory.Add(new OperatorDecisionLog(currentSequence, "Barcode Validation", decision, DateTime.UtcNow));

                            if (decision == LabelProcessingAction.Retry)
                                continue;

                            if (decision == LabelProcessingAction.Stop)
                                throw new OperationCanceledException();

                            if (decision == LabelProcessingAction.Ignore)
                                break; // skip the validation and move to next step
                        }
                    }

                    // OCR Validation
                    int ocrRetry = 0;
                    int cameraRetry = 0;
                    while (true)
                    {
                        token.ThrowIfCancellationRequested();

                        OnStatusChanged(LabelProcessingStatus.Capturing);

                        var snapshot = _cameraService.GetSnapshot();
                        if (snapshot == null)
                        {
                            cameraRetry++;
                            _logger.LogWarning("Camera snapshot failed. Attempt {Attempt}/{Max}.", cameraRetry, MaxRetryPerStep);

                            if (cameraRetry >= MaxRetryPerStep)
                            {
                                _logger.LogError("Camera unavailable after multiple attempts.");
                                OnStatusChanged(LabelProcessingStatus.CameraUnavailable);
                                try
                                {
                                    const int cameraRecoveryDelayMs = 3000;
                                    _logger.LogInformation("Waiting before camera reinitialization...");
                                    await Task.Delay(cameraRecoveryDelayMs, token);

                                    
                                }
                                catch (OperationCanceledException)
                                {
                                    _logger.LogInformation("Camera initialization cancelled.");
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Camera reinitialization failed.");
                                    OnStatusChanged(LabelProcessingStatus.Error);
                                }
                            }
                            await Task.Delay(100, token);
                            continue;
                        }

                        using (snapshot)
                        {
                            OnStatusChanged(LabelProcessingStatus.ReadingOcr);
                            var ocrResult = await _ocrService.ReadTextAsync(
                                   snapshot,
                                   viewerSize,
                                   coordinates.X,
                                   coordinates.Y,
                                   coordinates.Width,
                                   coordinates.Height,
                                   coordinates.ImageWidth,
                                   coordinates.ImageHeight);

                            var expectedSequence = labelData[request.SequenceVariableName];
                            if (string.Equals(ocrResult?.Trim(), expectedSequence.Trim(), StringComparison.Ordinal))
                            {
                                break; // OCR valid
                            }

                            ocrRetry++;
                            _logger.LogWarning("OCR mismatch. Attempt {Attempt}/{Max}. Expected {Expected}, Got {Actual}",
                                ocrRetry, MaxRetryPerStep, expectedSequence, ocrResult);

                            if (ocrRetry >= MaxRetryPerStep)
                            {
                                _logger.LogError("Max OCR retries reached for sequence {Sequence}.", currentSequence);

                                OnStatusChanged(LabelProcessingStatus.WaitingForOperator);

                                var decision = await WaitForOperatorDecisionAsync(token);

                                _decisionHistory.Add(new OperatorDecisionLog(currentSequence, "OCR Validation", decision, DateTime.UtcNow));

                                if (decision == LabelProcessingAction.Retry)
                                    continue;

                                if (decision == LabelProcessingAction.Stop)
                                    throw new OperationCanceledException();

                                if (decision == LabelProcessingAction.Ignore)
                                    break; // skip the validation and move to next step
                            }
                        }
                    }
                    // Save sequence to database or log for traceability


                    // Success -> Next label
                    printedCount++;
                    currentSequence++;
                    OnStatusChanged(LabelProcessingStatus.Completed);
                }
                OnStatusChanged(LabelProcessingStatus.Ready);
            }
            catch (OperationCanceledException)
            {
                OnStatusChanged(LabelProcessingStatus.Stopped);
            }
            finally
            {
                _lock.Release();
            }

        }

        private Task<string?> WaitForBarcodeAsync(CancellationToken token)
        {
            var tcs = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);

            CancellationTokenRegistration registration = default;

            void Cleanup()
            {
                _barcodeService.BarcodeScanned -= Handler;
                registration.Dispose();
            }

            void Handler(object? sender, BarcodeScannedEventArgs e)
            {
                Cleanup();
                tcs.TrySetResult(e.Value);
            }

            _barcodeService.BarcodeScanned += Handler;

            registration = token.Register(() =>
            {
                Cleanup();
                tcs.TrySetCanceled(token);
            });

            _ = _barcodeService.ScanAsync();

            return tcs.Task;
        }


        //Helper for user decision
        private TaskCompletionSource<LabelProcessingAction>? _operatorDecisionTcs;

        private Task<LabelProcessingAction> WaitForOperatorDecisionAsync(CancellationToken token)
        {
            _operatorDecisionTcs?.TrySetCanceled();

            _operatorDecisionTcs = new TaskCompletionSource<LabelProcessingAction>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            // Cancel if job is cancelled
            token.Register(() =>
            {
                _operatorDecisionTcs?.TrySetCanceled(token);
            });

            return _operatorDecisionTcs.Task;
        }
        public void SetUserDecision(LabelProcessingAction action)
        {
            _operatorDecisionTcs?.TrySetResult(action);
        }

    }

    public record OperatorDecisionLog(int Sequence, string Step, LabelProcessingAction Action, DateTime Timestamp);
}
