using System.Drawing;
using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Messaging;
using LASYS.Application.Contracts;
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
        public event EventHandler<OperatorDecisionRequiredEventArgs>? DecisionRequired;

        public event EventHandler<LogEventArgs>? LogGenerated;
        public event EventHandler<PrintingState>? PrintControlsStateChanged;
        public event EventHandler<DeviceStatusEventArgs>? DeviceStatusChanged;

        private volatile bool _isPaused;
        private TaskCompletionSource<bool>? _pauseTcs;

        private CancellationTokenSource? _printingCts;

        private DeviceStatusEventArgs? _lastPrinterStatus;
        private DeviceStatusEventArgs? _lastCameraStatus;
        private DeviceStatusEventArgs? _lastBarcodeStatus;
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

            _printerService.LabelStatusChanged += OnLabelStatusChanged;
            _printerService.PrinterStatusChanged += OnPrinterStatusChanged;

            _cameraService.CameraStatusChanged += OnCameraStatusChanged;
            _barcodeService.BarcodeStatusChanged += OnBarcodeStatusChanged;

        }

        public async Task InitializeDevicesAsync()
        {
            var config = await _cameraService.LoadAsync();
            var cameraIndex = _cameraService.GetCameraIndex(config.Name);

            await _cameraService.InitializeAsync();
            await _printerService.InitializeAsync();
            await _barcodeService.InitializeAsync();
        }

        public IEnumerable<DeviceStatusEventArgs> GetCurrentDeviceStatuses()
        {
            if (_lastPrinterStatus != null) yield return _lastPrinterStatus;
            if (_lastCameraStatus != null) yield return _lastCameraStatus;
            if (_lastBarcodeStatus != null) yield return _lastBarcodeStatus;
        }
        private void OnBarcodeStatusChanged(object? sender, BarcodeStatusEventArgs e)
        {
            switch (e.Status)
            {
                case BarcodeStatus.BarcodeNotConfigured:
                    break;
                case BarcodeStatus.BarcodeCommunicating:
                    break;
                case BarcodeStatus.BarcodeConnected:
                    break;
                case BarcodeStatus.BarcodeReconnecting:
                    break;
                case BarcodeStatus.BarcodeScanning:
                    break;
                case BarcodeStatus.BarcodeScanned:
                    break;
                case BarcodeStatus.BarcodeNotDetected:
                    break;
                case BarcodeStatus.BarcodeDisconnected:
                    break;
                case BarcodeStatus.BarcodeTimeout:
                    break;
                case BarcodeStatus.BarcodeError:
                    break;
                default:
                    break;
            }
            _lastBarcodeStatus = new DeviceStatusEventArgs(DeviceType.Barcode, e.Message, e.Description);
            DeviceStatusChanged?.Invoke(this, _lastBarcodeStatus);
        }
        private void OnCameraStatusChanged(object? sender, CameraStatusEventArgs e)
        {
            _lastCameraStatus = new DeviceStatusEventArgs(DeviceType.Camera, e.Message, e.Description);
            DeviceStatusChanged?.Invoke(this, _lastCameraStatus);
        }
        private void OnPrinterStatusChanged(object? sender, PrinterStatusEventArgs e)
        {
            _lastPrinterStatus = new DeviceStatusEventArgs(DeviceType.Printer, e.Message, e.Description);
            DeviceStatusChanged?.Invoke(this, _lastPrinterStatus);
            switch (e.Status)
            {
                case PrinterStatus.PrinterPaused:
                    LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Warning, e.Description));
                    break;
                case PrinterStatus.PrinterDisconnected:
                case PrinterStatus.PrintFailed:
                case PrinterStatus.PrinterNotDetected:
                case PrinterStatus.PrinterOffline:
                case PrinterStatus.PrinterError:
                case PrinterStatus.PrinterNotConfigured:
                    LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Error, e.Description));
                    break;
                default:
                    LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, e.Description));
                    break;
            }
        }

        private void OnLabelStatusChanged(object? sender, LabelEventArgs e)
        {
            switch (e.Status)
            {
                case LabelStatus.TemplateLoaded:
                    PrintControlsStateChanged?.Invoke(this, PrintingState.Idle);
                    LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, e.Description));
                    break;
                case LabelStatus.TemplateLoadFailed:
                    PrintControlsStateChanged?.Invoke(this, PrintingState.Disabled);
                    LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Error, e.Description));
                    break;
                default:
                    LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, e.Description));
                    break;
            }
        }

        public void LoadLabelTemplateAsync(string templatePath)
        {
            PrintControlsStateChanged?.Invoke(this, PrintingState.Initializing);
            try
            {
                _printerService.LoadLabelTemplate(templatePath);
            }
            catch (Exception ex)
            {
                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Error, ex.Message));
            }
            PrintControlsStateChanged?.Invoke(this, PrintingState.Idle); //temporary - for testing only
        }
        private async Task WaitIfPausedAsync(CancellationToken token)
        {
            if (!_isPaused)
                return;

            var tcs = _pauseTcs;

            if (tcs != null)
                await tcs.Task.WaitAsync(token);
        }
        public async Task StartJobAsync(Size viewerSize, StartLabelJobRequest request)
        {
            //DecisionRequired?.Invoke(this, new OperatorDecisionRequiredEventArgs(
            //                      ValidationFailure.BarcodeMismatch,
            //                      sequenceNo: 1,
            //                      barcodeResult: "test"));

            if (!await _lock.WaitAsync(0))
            {
                _logger.LogWarning("Attempted to start job while another job is running.");
                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Warning, "A label processing job is already in progress."));
                return; // Exit if already running
            }

            _printingCts?.Cancel();
            _printingCts = new CancellationTokenSource();
            var token = _printingCts.Token;

            int printedCount = 0;
            int currentSequence = request.StartSequence;

            LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, "Preparing print job"));

            try
            {
                var calibrationResult = await _calibrationService.LoadAsync();
                if (calibrationResult.Products == null || calibrationResult.Products.Count == 0)
                {
                    _logger.LogWarning("No calibration data found.");
                    LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Error, "No calibration data available."));
                    return;
                }

                var product = calibrationResult.Products.FirstOrDefault(p => p.ItemCode == request.ItemCode);
                if (product == null)
                {
                    _logger.LogWarning("ItemCode {ItemCode} not found in calibration.", request.ItemCode);
                    LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Error, "Selected item is not configured in calibration data."));
                    return;
                }

                var coordinates = product.Coordinates;

                while (printedCount < request.Quantity)
                {
                    token.ThrowIfCancellationRequested();

                    await WaitIfPausedAsync(token); // Check for pause between labels
                    // Print label
                    LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Printing label with sequence {currentSequence}"));

                    var labelData = new Dictionary<string, string>(request.LabelData);
                    labelData[request.SequenceVariableName] = currentSequence.ToString();

                    bool generated = _printerService.PrintLabelWithPreview($"LBL_{currentSequence}");
                    if (!generated)
                    {
                        return;
                    }

                    PrintControlsStateChanged?.Invoke(this, PrintingState.Printing);
                    _printerService.Print();
                    await Task.Delay(150, token);

                    // Barcode Validation
                    int barcodeRetry = 0;
                    while (true)
                    {
                        token.ThrowIfCancellationRequested();

                        await WaitIfPausedAsync(token); // Check for pause between retries

                        var scannedBarcode = await WaitForBarcodeAsync(token);
                        var expectedBarcode = labelData[request.BarcodeVariableName];

                        if (string.Equals(scannedBarcode?.Trim(), expectedBarcode.Trim(), StringComparison.Ordinal))
                        {
                            break; // barcode valid
                        }

                        barcodeRetry++;

                        LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Warning,
                            $"Barcode mismatch. Attempt {barcodeRetry}/{MaxRetryPerStep}. Expected {expectedBarcode}, Got {scannedBarcode}"));


                        if (barcodeRetry >= MaxRetryPerStep)
                        {
                            LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Error, "Max barcode retries reached."));
                            await Task.Delay(200);
                            LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, "Awaiting operator action..."));

                            var decision = await WaitForOperatorDecisionAsync(
                                new OperatorDecisionRequiredEventArgs(
                                    ValidationFailure.BarcodeMismatch,
                                    sequenceNo: currentSequence,
                                    barcodeResult: scannedBarcode),
                                token);

                            _decisionHistory.Add(new OperatorDecisionLog(currentSequence, "Barcode Validation", decision, DateTime.UtcNow));

                            if (decision == OperatorDecision.Retry)
                                continue;

                            if (decision == OperatorDecision.Stop)
                                throw new OperationCanceledException();

                            if (decision == OperatorDecision.Skip)
                                break; // skip the validation and move to next step
                        }
                    }

                    // OCR Validation
                    int ocrRetry = 0;
                    int cameraRetry = 0;
                    while (true)
                    {
                        token.ThrowIfCancellationRequested();

                        await WaitIfPausedAsync(token); // Check for pause between retries

                        LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, "Capturing image from camera..."));

                        var snapshot = _cameraService.GetSnapshot();
                        if (snapshot == null)
                        {
                            cameraRetry++;
                            LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Warning, $"Camera snapshot failed. Attempt {cameraRetry}/{MaxRetryPerStep}."));

                            if (cameraRetry >= MaxRetryPerStep)
                            {
                                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Error, "Camera unavailable after multiple attempts."));

                                try
                                {
                                    const int cameraRecoveryDelayMs = 3000;
                                    LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, "Waiting before camera reinitialization..."));
                                    await Task.Delay(cameraRecoveryDelayMs, token);

                                }
                                catch (OperationCanceledException)
                                {
                                    LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Error, "Camera initialization cancelled."));
                                }
                                catch (Exception ex)
                                {
                                    LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Error, ex.Message));
                                }
                            }
                            await Task.Delay(100, token);
                            continue;
                        }

                        using (snapshot)
                        {
                            LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, "Processing text recognition..."));
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

                            LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Warning, $"OCR mismatch. Attempt {ocrRetry}/{MaxRetryPerStep}. Expected {expectedSequence}, Got {ocrResult}"));

                            if (ocrRetry >= MaxRetryPerStep)
                            {
                                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Error, $"Max OCR retries reached for sequence {currentSequence}."));
                                await Task.Delay(200);
                                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, "Awaiting operator action..."));

                                var decision = await WaitForOperatorDecisionAsync(
                                    new OperatorDecisionRequiredEventArgs(
                                        ValidationFailure.OcrMismatch,
                                        sequenceNo: currentSequence,
                                        ocrResult: ocrResult),
                                    token);

                                _decisionHistory.Add(new OperatorDecisionLog(currentSequence, "OCR Validation", decision, DateTime.UtcNow));

                                if (decision == OperatorDecision.Retry)
                                    continue;

                                if (decision == OperatorDecision.Stop)
                                    throw new OperationCanceledException();

                                if (decision == OperatorDecision.Skip)
                                    break; // skip the validation and move to next step
                            }
                        }
                    }
                    // Save sequence to database or log for traceability


                    // Success -> Next label
                    printedCount++;
                    currentSequence++;
                }
                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, "Print completed successfully."));
                PrintControlsStateChanged?.Invoke(this, PrintingState.Completed);
            }
            catch (OperationCanceledException)
            {
                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Warning, "Operation stopped by operator."));
                PrintControlsStateChanged?.Invoke(this, PrintingState.Stopped);
            }
            finally
            {
                _lock.Release();
                PrintControlsStateChanged?.Invoke(this, PrintingState.Idle);
            }

        }

        private Task<string?> WaitForBarcodeAsync(CancellationToken token)
        {
            LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, "Scanning barcode..."));

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
        private TaskCompletionSource<OperatorDecision>? _operatorDecisionTcs;

        private Task<OperatorDecision> WaitForOperatorDecisionAsync(OperatorDecisionRequiredEventArgs eventArgs, CancellationToken token)
        {
            _operatorDecisionTcs?.TrySetCanceled();

            _operatorDecisionTcs = new TaskCompletionSource<OperatorDecision>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            // Notify UI that operator decision is required
            DecisionRequired?.Invoke(this, eventArgs);

            // Cancel if job is cancelled
            token.Register(() =>
            {
                _operatorDecisionTcs?.TrySetCanceled(token);
            });

            return _operatorDecisionTcs.Task;
        }
        public void SetUserDecision(OperatorDecision action)
        {
            _operatorDecisionTcs?.TrySetResult(action);
        }

        public void Pause()
        {
            if (_isPaused)
                return;

            _isPaused = true;
            _pauseTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Warning, "Operation paused by operator."));
            PrintControlsStateChanged?.Invoke(this, PrintingState.Paused);
        }

        public void Resume()
        {
            if (!_isPaused)
                return;

            _isPaused = false;
            _pauseTcs?.TrySetResult(true);
            LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Warning, "Operation resumed by operator."));
            PrintControlsStateChanged?.Invoke(this, PrintingState.Resumed);
        }

        public void Stop()
        {
            _printingCts?.Cancel();
        }

        public void NotifyStatus(PrintingState status)
        {
            throw new NotImplementedException();
        }


    }

    public record OperatorDecisionLog(int Sequence, string Step, OperatorDecision Action, DateTime Timestamp);
}
