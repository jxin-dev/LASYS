//using LASYS.Application.Common.Enums;
using System.Diagnostics;
using LASYS.Application.Common.Mappings;
using LASYS.Application.Common.Messaging;
using LASYS.Application.Common.Utilities;
using LASYS.Application.Events;
using LASYS.Application.Features.BarcodeValidation;
using LASYS.Application.Features.BarcodeValidation.ValidateLabelBarcode;
using LASYS.Application.Features.BatchPrinting.Enums;
using LASYS.Application.Features.BatchPrinting.Events;
using LASYS.Application.Features.BatchPrinting.Helpers;
using LASYS.Application.Features.BatchPrinting.Models;
using LASYS.Application.Features.LabelInstructions.GetLabelInstructionContext;
using LASYS.Application.Interfaces.Context;
using LASYS.Application.Interfaces.Persistence.Repositories;
using LASYS.Application.Interfaces.Services;
using LASYS.Application.Interfaces.Services.NiceLabel;
using LASYS.Application.Models.Hardware.Printer;
using MediatR;

namespace LASYS.Application.Features.BatchPrinting.Services
{
    public sealed class BatchPrintProcessService : IBatchPrintProcessService
    {
        private readonly ICurrentUser _currentUser;
        private readonly IPrintJobController _jobController;
        private readonly INiceLabelTemplateService _niceLabelTemplateService;
        private readonly IDeviceManager _deviceManager;
        private readonly IOCRService _ocrService;
        private readonly ICalibrationService _calibrationService;
        private readonly IPrintLabelRepository _printLabelRepository;
        private readonly IMediator _mediator;
        private readonly ILabelPreviewHub _labelPreviewHub;
        private readonly IIpAddressProvider _ipAddressProvider;

        private TaskCompletionSource<StepResult>? _decisionTcs;
        private TaskCompletionSource<ApprovalAuthorizationResult>? _approvalTcs;

        public event EventHandler<OperatorDecisionRequiredEventArgs>? OperatorDecisionRequired;
        public event EventHandler<PrintJobState>? JobStateChanged;
        public event EventHandler<LogEventArgs>? LogGenerated;
        public event EventHandler? ApprovalAuthorizationRequired;

        public BatchPrintProcessService(ICurrentUser currentUser, IPrintJobController jobController, INiceLabelTemplateService niceLabelTemplateService, IDeviceManager deviceManager, IPrintLabelRepository printLabelRepository, IMediator mediator, ILabelPreviewHub labelPreviewHub, IOCRService ocrService, ICalibrationService calibrationService, IIpAddressProvider ipAddressProvider)
        {
            _currentUser = currentUser;
            _jobController = jobController;
            _niceLabelTemplateService = niceLabelTemplateService;
            _deviceManager = deviceManager;
            _printLabelRepository = printLabelRepository;
            _mediator = mediator;
            _labelPreviewHub = labelPreviewHub;
            _ocrService = ocrService;
            _calibrationService = calibrationService;
            _ipAddressProvider = ipAddressProvider;
        }
        public PrintJobState? GetJob(Guid jobId)
        {
            return _jobController.GetJob(jobId);
        }
        private void NotifyJobStateChanged(Guid jobId)
        {
            var job = _jobController.GetJob(jobId);

            if (job is not null)
                JobStateChanged?.Invoke(this, job);
        }
        private Guid _activeJobId;
        public Task<PrintJobState> InitializeAsync(LabelPrintingContext context)
        {
            var printerName = _deviceManager.Printer.PrinterName ?? "Unknown";
            var jobId = _jobController.CreateJob(printerName, context);
            _activeJobId = jobId;

            var job = _jobController.GetJob(jobId)!;
            if (job.RemainingQuantity == 0)
            {
                _jobController.Printed(jobId);
            }

            var isTemplateLoaded = _niceLabelTemplateService.IsTemplateLoaded;
            if (!isTemplateLoaded)
            {
                _niceLabelTemplateService.LoadTemplate(job.NiceLabelFilePath);
            }

            NotifyJobStateChanged(jobId);

            return Task.FromResult(job);
        }

        public Task StartAsync(Guid jobId, int quantity, bool endOfBatch)
        {
            var job = _jobController.GetJob(jobId);
            job.SetQuantity(quantity);
            job.SetEndOfBatch(endOfBatch);

            NotifyJobStateChanged(jobId);

            _ = Task.Run(async () => await RunAsync(jobId, job.CancellationTokenSource.Token));

            return Task.CompletedTask;
        }
        private void EnsureCanContinue(PrintJobState job)
        {
            var token = job.CancellationTokenSource.Token;

            token.ThrowIfCancellationRequested();

            if (job.Status == PrintJobStatus.Paused)
            {
                job.ResumeSignal.Wait(token);
            }

            token.ThrowIfCancellationRequested();

            if (job.Status == PrintJobStatus.Stopped)
            {
                throw new OperationCanceledException();
            }
        }
        private async Task RunAsync(Guid jobId, CancellationToken cancellationToken)
        {

            var job = _jobController.GetJob(jobId);
            if (job is null) return;

            job.InProgress();

            try
            {

                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Started print job. Total quantity: {job.TotalQuantity}"));
                var startSequence = job.Context.PrintDetails!.NextSequence;

                var latestSpecialStatus = await _printLabelRepository.GetLatestSpecialLabelStatusAsync(job.ItemCode,job.LotNo, job.BoxType);
                bool hasOpenBatch = latestSpecialStatus == "First";

                while (job.Context.PrintDetails!.NextSequence <= (job.TotalQuantity + startSequence) - 1) // 1 - 50
                {
                    var prnFileLocation = string.Empty;
                    EnsureCanContinue(job);
                    // Generate Label Files Once per sequence, regardless of pair count.
                    var generationAttempt = 0;
                    while (true)
                    {
                        generationAttempt++;
                        if (generationAttempt == 1)
                        {
                            LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Starting label files generation attempt for label {job.CurrentSequenceFormat}."));
                        }
                        else
                        {
                            LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Retrying label files generation attempt for label {job.CurrentSequenceFormat}. Attempt {generationAttempt}."));
                        }
                        var generationLabelFilesResult = await GenerateLabelFilesAsync(job, job.Context.PrintDetails!.NextSequence, cancellationToken);
                        if (generationLabelFilesResult.Result == StepResult.Success)
                        {
                            job.MarkGenerated();
                            prnFileLocation = generationLabelFilesResult.PrnPath;

                            LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Label files generated successfully for label {job.CurrentSequenceFormat}."));
                            break;
                        }
                        if (generationLabelFilesResult.Result == StepResult.Retry)
                        {
                            continue;
                        }
                        if (generationLabelFilesResult.Result == StepResult.Stop)
                        {
                            LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Error, $"Label generation failed. Job stopped by {_currentUser.FullName} on label {job.CurrentSequenceFormat}."));
                            _jobController.Stop(jobId);
                            EnsureCanContinue(job);
                        }
                        break;
                    }

                    var stopRequested = false;
                    var pairCount = job.IsPairedType ? 2 : 1;
                    var completedPairs = 0;
                    foreach (var pairIndex in Enumerable.Range(1, pairCount))
                    {

                        job.SetCurrentPair(pairIndex, pairCount);

                        var pairText = FormatPair(pairIndex, pairCount);

                        // PRINT
                        var printAttempt = 0;
                        while (true)
                        {
                            printAttempt++;
                            if (printAttempt == 1)
                            {
                                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Starting print attempt for label {job.CurrentSequenceFormat}{pairText}."));
                            }
                            else
                            {
                                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Retrying print attempt for label {job.CurrentSequenceFormat}{pairText}. Attempt {printAttempt}."));
                            }
                            var validationPrintResult = await ValidatePrintAsync(job, prnFileLocation, pairIndex, pairCount, cancellationToken);
                            if (validationPrintResult == StepResult.Success)
                            {
                                job.MarkPrinted();
                                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Label {job.CurrentSequenceFormat}{pairText} printed successfully."));
                                break;
                            }
                            if (validationPrintResult == StepResult.Retry)
                            {
                                continue;
                            }
                            if (validationPrintResult == StepResult.Stop)
                            {
                                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Error, $"Print operation failed. Job stopped {_currentUser.FullName} on label {job.CurrentSequenceFormat}{pairText}."));
                                stopRequested = true;
                                _jobController.Stop(jobId);
                                EnsureCanContinue(job);
                            }
                            break;
                        }
                        await Task.Delay(1500, cancellationToken); // add delay to make sure the label is already printed
                        // BARCODE VALIDATION
                        var barcodeAttempt = 0;
                        while (true)
                        {
                            barcodeAttempt++;
                            if (barcodeAttempt == 1)
                            {
                                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Starting barcode validation for label {job.CurrentSequenceFormat}{pairText}."));
                            }
                            else
                            {
                                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Retrying barcode validation for label {job.CurrentSequenceFormat}{pairText}. Attempt {barcodeAttempt}."));
                            }
                            var validationBarcodeResult = await ValidateBarcodeAsync(job, pairIndex, pairCount, cancellationToken);
                            if (validationBarcodeResult == StepResult.Success)
                            {
                                job.MarkBarcodeValidated();
                                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Barcode validation passed for label {job.CurrentSequenceFormat}{pairText}."));
                                break;
                            }
                            if (validationBarcodeResult == StepResult.Retry)
                            {
                                continue;
                            }
                            if (validationBarcodeResult == StepResult.Stop)
                            {
                                await SaveFailedLabelAsync(job);
                                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Error, $"Barcode validation failed. Job stopped by {_currentUser.FullName} on label {job.CurrentSequenceFormat}{pairText}."));
                                stopRequested = true;
                                _jobController.Stop(jobId);
                                EnsureCanContinue(job);
                            }
                            break;
                        }
                        await Task.Delay(500, cancellationToken);
                        // OCR VALIDATION
                        var ocrAttempt = 0;
                        while (true)
                        {
                            ocrAttempt++;
                            if (ocrAttempt == 1)
                            {
                                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Starting OCR validation for label {job.CurrentSequenceFormat}{pairText}."));
                            }
                            else
                            {
                                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Retrying OCR validation for label {job.CurrentSequenceFormat}{pairText}. Attempt {ocrAttempt}."));
                            }

                            var validationOcrResult = await ValidateOcrAsync(job, pairIndex, pairCount, cancellationToken);
                            if (validationOcrResult == StepResult.Success)
                            {
                                job.MarkOcrValidated();
                                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"OCR validation passed for label {job.CurrentSequenceFormat}{pairText}."));
                                break;
                            }
                            if (validationOcrResult == StepResult.Retry)
                            {
                                continue;
                            }
                            if (validationOcrResult == StepResult.Skip) // Add reason for skip?
                            {
                                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Warning, $"OCR validation skipped by {_currentUser.FullName} for label {job.CurrentSequenceFormat}{pairText} after {ocrAttempt} attempt(s)."));
                                break;
                            }
                            if (validationOcrResult == StepResult.Stop)
                            {
                                await SaveFailedLabelAsync(job);
                                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Error, $"OCR validation failed. Job stopped by {_currentUser.FullName} on label {job.CurrentSequenceFormat}{pairText}."));
                                stopRequested = true;
                                _jobController.Stop(jobId);
                                EnsureCanContinue(job);
                            }
                            break; //
                        }

                        bool isFirstLabel =job.Context.PrintDetails!.NextSequence == startSequence;

                        bool isLastLabel = job.Context.PrintDetails!.NextSequence == (startSequence + job.TotalQuantity - 1);

                        await PrepareLabelStatusAsync(
                            job,
                            jobId,
                            hasOpenBatch,
                            isFirstLabel,
                            isLastLabel,
                            job.EndOfBatch,
                            cancellationToken);

                        //Save Data 
                        var saveAttempt = 0;
                        while (true)
                        {
                            saveAttempt++;
                            if (saveAttempt == 1)
                            {
                                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Starting save operation for label {job.CurrentSequenceFormat}{pairText}."));
                            }
                            else
                            {
                                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Retrying save operation for label {job.CurrentSequenceFormat}{pairText}. Attempt {saveAttempt}."));
                            }
                            var saveResult = await SavePrintedLabelAsync(job, cancellationToken);
                            if (saveResult == StepResult.Success)
                            {
                                var hasSampleLabel = (!hasOpenBatch && isFirstLabel) || (isLastLabel && job.EndOfBatch);
                                job.MarkSaved(hasSampleLabel);
                                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Save operation completed for label {job.CurrentSequenceFormat}{pairText}."));

                                completedPairs++;
                                if (completedPairs == pairCount)
                                {
                                    job.MoveToNextLabel();
                                    NotifyJobStateChanged(job.JobId);
                                }

                                break;
                            }
                            if (saveResult == StepResult.Retry)
                            {
                                continue;
                            }
                            if (saveResult == StepResult.Stop)
                            {
                                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Error, $"Save operation failed. Job stopped by {_currentUser.FullName} on label {job.CurrentSequenceFormat}{pairText}."));
                                stopRequested = true;
                                _jobController.Stop(jobId);
                                EnsureCanContinue(job);
                            }
                            break;
                        }
                    }

                    if (stopRequested) break;
                }

                _jobController.Complete(jobId);
                NotifyJobStateChanged(jobId);
                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, "Batch printing completed."));

            }
            catch (OperationCanceledException)
            {
                var stage = job.CurrentStage;
                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Error, $"Batch printing operation stopped by {_currentUser.FullName}."));
                NotifyJobStateChanged(jobId);
            }
            finally
            {
                _jobController.Reset(jobId);
                NotifyJobStateChanged(jobId);
            }
        }

        private async Task<ApprovalAuthorizationResult> RequestApprovalAuthorizationAsync(CancellationToken cancellationToken)
        {
            _approvalTcs =
                new TaskCompletionSource<ApprovalAuthorizationResult>(
                    TaskCreationOptions.RunContinuationsAsynchronously);
            Debug.WriteLine("1. Raise event");

            ApprovalAuthorizationRequired?.Invoke(this, EventArgs.Empty);
            Debug.WriteLine("2. Waiting for approval...");

            try
            {
                var result = await _approvalTcs.Task.ConfigureAwait(false);

                Debug.WriteLine($"Returned: {result.IsApproved}");

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
            //using (cancellationToken.Register(() =>
            //    _approvalTcs.TrySetResult(new ApprovalAuthorizationResult(false))))
            //{
            //    return await _approvalTcs.Task;
            //}
        }


        public Task<ApprovalAuthorizationResult> RequestApprovalAsync(CancellationToken cancellationToken = default)
        {
            return RequestApprovalAuthorizationAsync(cancellationToken);
        }


        public void SetApprovalAuthorized(string userCode, string sectionId)
        {
            Debug.WriteLine("SetApprovalAuthorized");
            var success = _approvalTcs?.TrySetResult(new ApprovalAuthorizationResult(true, userCode, sectionId));

           Debug.WriteLine($"TrySetResult = {success}");
        }

        public void CancelApprovalAuthorization()
        {
            Debug.WriteLine("CancelApprovalAuthorization");
            var success = _approvalTcs?.TrySetResult(new ApprovalAuthorizationResult(false));
            Debug.WriteLine($"TrySetResult = {success}");
        }

        private static string FormatPair(int pairIndex, int pairCount)
        {
            return pairCount > 1 ? $" (Pair {pairIndex}/{pairCount})" : string.Empty;
        }
        private async Task<StepResult> RequestOperatorDecisionAsync(OperatorDecisionRequiredEventArgs args, CancellationToken cancellationToken)
        {
            _decisionTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

            OperatorDecisionRequired?.Invoke(this, args);
            //await Task.Delay(500, cancellationToken);

            using (cancellationToken.Register(() =>
            {
                _decisionTcs.TrySetResult(StepResult.Stop);
            }))
            {
                return await _decisionTcs.Task;
            }
        }

        private async Task PrepareLabelStatusAsync(PrintJobState job, Guid jobId, bool hasOpenBatch, bool isFirstLabel, bool isLastLabel, bool isEndOfBatch, CancellationToken cancellationToken)
        {
            job.ResetPrintType();

            if (isFirstLabel && !hasOpenBatch)
            {
                var approval = await RequestApprovalAuthorizationAsync(cancellationToken);

                if (!approval.IsApproved)
                {
                    _jobController.Stop(jobId, true);
                    EnsureCanContinue(job);
                }

                var ipAddress = _ipAddressProvider.GetLocalIpAddress();

                job.SetApproval(
                    approval.UserCode!,
                    approval.SectionId!,
                    ipAddress);

                job.MarkFirst();
                NotifyJobStateChanged(jobId);
            }
            else if (isLastLabel && isEndOfBatch)
            {
                var approval = await RequestApprovalAuthorizationAsync(cancellationToken);

                if (!approval.IsApproved)
                {
                    _jobController.Stop(jobId, true);
                    EnsureCanContinue(job);
                }

                var ipAddress = _ipAddressProvider.GetLocalIpAddress();

                job.SetApproval(
                    approval.UserCode!,
                    approval.SectionId!,
                    ipAddress);

                job.MarkLast();
                NotifyJobStateChanged(jobId);

            }
        }

        private async Task<StepResult> SavePrintedLabelAsync(PrintJobState job, CancellationToken cancellationToken)
        {
            EnsureCanContinue(job);

            var context = job.Context;
            var pairedType = job.CurrentPairCount == 1 ? "Single"
                : job.CurrentPairNumber == 1 ? "Paired1" : "Paired2";

            bool isSaved = await _printLabelRepository.SavePrintedLabelAsync(new SequenceData(
                context.MasterLabelDetails!.BoxType,
                job.PrinterName,
                context.LabelInstructionDetails!.ItemCode,
                context.LabelInstructionDetails.LotNo,
                context.PrintDetails!.NextSequence,
                pairedType,
                context.PrintDetails!.BatchNumber,
                context.PrintDetails!.SetNumber,
                job.CurrentLabelStatus,
                job.ApprovedByUserCode!,
                job.ApprovedBySectionId!,
                job.ApprovedByIpAddress!,
                job.ApprovedByDateTime!
                ));

            if (isSaved)
            {
                return StepResult.Success;
            }

            return await RequestOperatorDecisionAsync(new OperatorDecisionRequiredEventArgs(ValidationFailure.SaveFailed, job.CurrentSequenceFormat, job.CurrentPairNumber, job.PrintedCount), cancellationToken);

        }
        private async Task SaveFailedLabelAsync(PrintJobState job)
        {

            var context = job.Context;
            var pairedType = job.CurrentPairCount == 1 ? "Single"
                : job.CurrentPairNumber == 1 ? "Paired1" : "Paired2";

            job.MarkFailed();

            await _printLabelRepository.SavePrintedLabelAsync(new SequenceData(
                context.MasterLabelDetails!.BoxType,
                job.PrinterName,
                context.LabelInstructionDetails!.ItemCode,
                context.LabelInstructionDetails.LotNo,
                context.PrintDetails!.NextSequence,
                pairedType,
                context.PrintDetails!.BatchNumber,
                context.PrintDetails!.SetNumber,
                job.CurrentLabelStatus,
                job.ApprovedByUserCode!,
                job.ApprovedBySectionId!,
                job.ApprovedByIpAddress!,
                job.ApprovedByDateTime!
                ));


        }
        private async Task<(StepResult Result, string PrnPath)> GenerateLabelFilesAsync(PrintJobState job, long sequence, CancellationToken cancellationToken)
        {
            EnsureCanContinue(job);
            try
            {
                var formattedCurrentSequence = SequenceFormatter.Format(sequence, job.SequenceLength);

                var labelData = job.LabelData.Clone().Add("BOX_NO", formattedCurrentSequence);

                var templateVariables = _niceLabelTemplateService.GetTemplateVariables().ToHashSet(StringComparer.OrdinalIgnoreCase);

                var variablesToSet = labelData.Variables.Where(x => templateVariables.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);

                _niceLabelTemplateService.SetVariables(variablesToSet);

                var filename = PrintFileNameBuilder.Build(job.ItemCode, job.LotNo, formattedCurrentSequence);

                var isPreviewGenerated = _niceLabelTemplateService.GeneratePreview(job.Paths.LabelsDirectory, filename);

                if (!isPreviewGenerated)
                {
                    return (await RequestOperatorDecisionAsync(new OperatorDecisionRequiredEventArgs(ValidationFailure.FileGenerationFailed, job.CurrentSequenceFormat), cancellationToken), string.Empty);
                }

                var imagePath = Path.Combine(job.Paths.LabelsDirectory, $"{filename}.jpg");

                if (File.Exists(imagePath))
                {
                    _labelPreviewHub.Publish(imagePath);
                }

                var isPrnGenerated = _niceLabelTemplateService.GeneratePrn(job.Paths.LabelsDirectory, filename, out string prnPath);

                if (!isPrnGenerated || string.IsNullOrWhiteSpace(prnPath) || !File.Exists(prnPath))
                {
                    return (await RequestOperatorDecisionAsync(new OperatorDecisionRequiredEventArgs(ValidationFailure.FileGenerationFailed, job.CurrentSequenceFormat), cancellationToken), string.Empty);
                }
                return (StepResult.Success, prnPath);

            }
            catch (Exception ex)
            {
                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Error, $"Fatal error generating label files: {ex.Message}"));

                return (await RequestOperatorDecisionAsync(new OperatorDecisionRequiredEventArgs(ValidationFailure.FileGenerationFailed, job.CurrentSequenceFormat), cancellationToken), string.Empty);
            }

        }
        private async Task<StepResult> ValidatePrintAsync(PrintJobState job, string prnFileLocation, int pairNumber, int totalPairs, CancellationToken cancellationToken)
        {
            EnsureCanContinue(job);
            NotifyJobStateChanged(job.JobId);

            return StepResult.Success; //comment for real implementation

            var isPrinted = await _deviceManager.Printer.IsPrinted(prnFileLocation);
            if (isPrinted)
            {
                return StepResult.Success;
            }
            var connection = _deviceManager.Printer.Connection;

            string printerType = connection switch
            {
                SerialPrinterConnection serial => $"Serial ({serial.ComPort})",
                UsbPrinterConnection usb => $"USB ({usb.UsbId})",
                _ => "Unknown"
            };

            string printerDetails = $"Printer: {_deviceManager.Printer.PrinterName ?? "Unknown Printer"} \nConnection: {printerType}";

            LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Error, printerDetails));


            var decision = await RequestOperatorDecisionAsync(
                new OperatorDecisionRequiredEventArgs(
                    ValidationFailure.PrinterUnavailable,
                    job.CurrentSequenceFormat,
                    pairNumber,
                    totalPairs,
                    printerDetails: printerDetails),
                cancellationToken);

            if (decision == StepResult.Retry)
            {
                LogGenerated?.Invoke(this,
                    new LogEventArgs(
                        MessageType.Warning,
                        $"Reinitializing printer..."));

                await _deviceManager.Printer.InitializeAsync();

                return StepResult.Retry;
            }

            return decision;

        }
        private async Task<StepResult> ValidateBarcodeAsync(PrintJobState job, int pairNumber, int totalPairs, CancellationToken cancellationToken)
        {
            EnsureCanContinue(job);

            return StepResult.Success; //comment for real implementation

            // Ensure scanner is connected
            if (!_deviceManager.Barcode.IsConnected)
            {
                await _deviceManager.Barcode.InitializeAsync();

                if (!_deviceManager.Barcode.IsConnected)
                {
                    return await RequestOperatorDecisionAsync(
                        new OperatorDecisionRequiredEventArgs(
                            ValidationFailure.ScannerNotDetected,
                            job.CurrentSequenceFormat,
                            pairNumber,
                            totalPairs),
                        cancellationToken);
                }
            }


            var waitScannedTextTask = _deviceManager.Barcode.WaitForBarcodeAsync(cancellationToken);
            await _deviceManager.Barcode.ScanAsync();
            var barcodeScanned = await waitScannedTextTask;
            if (string.IsNullOrWhiteSpace(barcodeScanned))
            {
                return await RequestOperatorDecisionAsync(
                    new OperatorDecisionRequiredEventArgs(
                        ValidationFailure.BarcodeMismatch,
                        job.CurrentSequenceFormat,
                        pairNumber,
                        totalPairs),
                    cancellationToken);
            }

            bool isEumdr = job.Context.ProductDetails!.IsEumdr;
            var validationResult = await _mediator.Send(new ValidateLabelBarcodeQuery(barcodeScanned, isEumdr), cancellationToken);
            if (!validationResult.IsValid)
            {
                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Error, validationResult.ErrorMessage));

                return await RequestOperatorDecisionAsync(
                    new OperatorDecisionRequiredEventArgs(
                        ValidationFailure.BarcodeMismatch,
                        job.CurrentSequenceFormat,
                        pairNumber,
                        totalPairs),
                    cancellationToken);
            }

            var barcodeTypeValue = Enum.Parse<BarcodeType>(job.Context.ProductDetails!.BarcodeType, ignoreCase: true);
            int barcodeType = (int)barcodeTypeValue;

            var boxType = job.Context.MasterLabelDetails!.BoxType switch
            {
                Common.Enums.BoxType.CartonBox => "5",
                Common.Enums.BoxType.OuterCartonBox => "7",
                Common.Enums.BoxType.AdditionalCartonBox => "5",
                Common.Enums.BoxType.UnitBox => "3",
                Common.Enums.BoxType.AdditionalUnitBox => "3",
                Common.Enums.BoxType.OuterUnitBox => "4",
                _ => "1"
            };

            var barcodeNumber = $"{boxType}{job.Context.ProductDetails!.BarcodeNumber}";
            //var barcodeNumber = $"{barcodeType}{job.Context.ProductDetails!.BarcodeNumber}";


            if (!Matches(validationResult, "01", barcodeNumber))
            {
                return await RequestOperatorDecisionAsync(
                    new OperatorDecisionRequiredEventArgs(
                        ValidationFailure.BarcodeMismatch,
                        job.CurrentSequenceFormat,
                        pairNumber,
                        totalPairs),
                    cancellationToken);
            }
            if (!Matches(validationResult, "17", job.Context.LabelInstructionDetails!.ExpirationDate?.ToString(NiceLabelDataMappings.BarcodeDateFormat)!))
            {
                return await RequestOperatorDecisionAsync(
                    new OperatorDecisionRequiredEventArgs(
                        ValidationFailure.BarcodeMismatch,
                        job.CurrentSequenceFormat,
                        pairNumber,
                        totalPairs),
                    cancellationToken);
            }

            if (!Matches(validationResult, "10", job.Context.LabelInstructionDetails!.LotNo))
            {
                return await RequestOperatorDecisionAsync(
                    new OperatorDecisionRequiredEventArgs(
                        ValidationFailure.BarcodeMismatch,
                        job.CurrentSequenceFormat,
                        pairNumber,
                        totalPairs),
                    cancellationToken);
            }

            if (isEumdr &&
                !Matches(validationResult, "11", job.Context.LabelInstructionDetails!.ManufactureDate?.ToString(NiceLabelDataMappings.BarcodeDateFormat)!))
            {
                return await RequestOperatorDecisionAsync(
                    new OperatorDecisionRequiredEventArgs(
                        ValidationFailure.BarcodeMismatch,
                        job.CurrentSequenceFormat,
                        pairNumber,
                        totalPairs),
                    cancellationToken);
            }
            return StepResult.Success;
        }
        private static bool Matches(BarcodeValidationResult result, string ai, string expected)
        {
            return result.ApplicationIdentifiers.TryGetValue(ai, out var actual)
                && !string.IsNullOrEmpty(actual)
                && actual.Contains(expected, StringComparison.OrdinalIgnoreCase);
            //return result.ApplicationIdentifiers.TryGetValue(
            //    ai, out var actual) && string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
        }
        private async Task<StepResult> ValidateOcrAsync(PrintJobState job, int pairNumber, int totalPairs, CancellationToken cancellationToken)
        {
            EnsureCanContinue(job);

            return StepResult.Success; //comment for real implementation

            if (!_deviceManager.Camera.IsCameraReady())
            {
                var connected = await _deviceManager.Camera.ReconnectAsync();

                if (!connected)
                {
                    return await RequestOperatorDecisionAsync(
                        new OperatorDecisionRequiredEventArgs(
                            ValidationFailure.CameraNotDetected,
                            job.CurrentSequenceFormat,
                            pairNumber,
                            totalPairs),
                        cancellationToken);
                }
                return StepResult.Retry;
            }

            var coordinates = await _calibrationService.GetCoordinatesAsync(job.ItemCode, job.Revision, job.BoxType);
            if (coordinates == null)
            {
                return await RequestOperatorDecisionAsync(
                    new OperatorDecisionRequiredEventArgs(
                        ValidationFailure.OcrCalibrationNotFound,
                        job.CurrentSequenceFormat,
                        pairNumber,
                        totalPairs),
                    cancellationToken);
            }
            var snapshot = _deviceManager.Camera.GetSnapshot();
            if (snapshot == null)
            {
                return await RequestOperatorDecisionAsync(
                    new OperatorDecisionRequiredEventArgs(
                        ValidationFailure.OcrMismatch,
                        job.CurrentSequenceFormat,
                        pairNumber,
                        totalPairs),
                    cancellationToken);
            }

            var result = await _ocrService.ReadTextAsync(snapshot, coordinates);
            if (result != job.CurrentSequenceFormat)
            {
                return await RequestOperatorDecisionAsync(new OperatorDecisionRequiredEventArgs(ValidationFailure.OcrMismatch, job.CurrentSequenceFormat, pairNumber, totalPairs, ocrResult: result), cancellationToken);
            }
            return StepResult.Success;
        }

        public void Pause(Guid jobId)
        {
            var job = _jobController.GetJob(jobId);

            if (job is null)
                return;

            _jobController.Pause(jobId);
            NotifyJobStateChanged(jobId);
        }
        public void Resume(Guid jobId)
        {
            var job = _jobController.GetJob(jobId);

            if (job is null)
                return;

            _jobController.Resume(jobId);
            NotifyJobStateChanged(jobId);
        }
        public void Stop(Guid jobId, bool hasApproval = false)
        {
            _jobController.Stop(jobId, hasApproval);
            NotifyJobStateChanged(jobId);

            _decisionTcs?.TrySetResult(StepResult.Stop);
        }
        public void SetUserDecision(StepResult decision)
        {
            _decisionTcs?.TrySetResult(decision);
        }

        public async Task<bool> HasOpenBatchAsync()
        {
            var job = _jobController.GetJob(_activeJobId);
            var latestSpecialStatus = await _printLabelRepository.GetLatestSpecialLabelStatusAsync(job.ItemCode, job.LotNo, job.BoxType);
            bool hasOpenBatch = latestSpecialStatus == "First";
            return hasOpenBatch;
        }
    }
}
