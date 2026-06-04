using LASYS.Application.Common.Messaging;
using LASYS.Application.Common.Utilities;
using LASYS.Application.Events;
using LASYS.Application.Features.BatchPrinting.Enums;
using LASYS.Application.Features.BatchPrinting.Events;
using LASYS.Application.Features.BatchPrinting.Helpers;
using LASYS.Application.Features.BatchPrinting.Models;
using LASYS.Application.Features.Devices.Events;
using LASYS.Application.Features.LabelInstructions.GetLabelInstructionContext;
using LASYS.Application.Interfaces.Context;
using LASYS.Application.Interfaces.Services;

namespace LASYS.Application.Features.BatchPrinting.Services
{
    public sealed class BatchPrintProcessService : IBatchPrintProcessService
    {
        private readonly ICurrentUser _currentUser;
        private readonly IPrintJobController _jobController;
        private readonly INiceLabelTemplateService _niceLabelTemplateService;
        private readonly IDeviceManager _deviceManager;
        //private readonly IPrinterService _printerService;
        //private readonly IBarcodeService _barcodeService;
        //private readonly IOCRService _ocrService;

        private TaskCompletionSource<StepResult>? _decisionTcs;

        public event EventHandler<OperatorDecisionRequiredEventArgs>? OperatorDecisionRequired;
        public event EventHandler<PrintJobState>? JobStateChanged;
        public event EventHandler<LogEventArgs>? LogGenerated;

        public BatchPrintProcessService(ICurrentUser currentUser, IPrintJobController jobController, INiceLabelTemplateService niceLabelTemplateService, IDeviceManager deviceManager)
        {
            _currentUser = currentUser;
            _jobController = jobController;
            _niceLabelTemplateService = niceLabelTemplateService;
            _deviceManager = deviceManager;

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
        public Task<Guid> StartAsync(LabelPrintingContext context, int quantity)
        {
            var jobId = _jobController.CreateJob(context, quantity);
            var job = _jobController.GetJob(jobId)!;

            NotifyJobStateChanged(jobId);

            _ = Task.Run(async () => await RunAsync(jobId, job.CancellationTokenSource.Token));

            return Task.FromResult(jobId);
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

            try
            {
                var isTemplateLoaded = _niceLabelTemplateService.IsTemplateLoaded;
                if (!isTemplateLoaded)
                {
                    _niceLabelTemplateService.LoadTemplate(job.NiceLabelFilePath);
                }


                var sequence = job.StartSequenceToPrint;

                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Started print job. Total quantity: {job.TotalQuantity}"));

                var prnFileLocation = string.Empty;
                while (sequence <= job.TotalQuantity)
                {
                    EnsureCanContinue(job);

                    var formattedCurrentSequence = SequenceFormatter.Format(sequence, 5);
                    var formattedLastSequence = SequenceFormatter.Format(job.LastSequenceToPrint, 5);

                    var generationAttempt = 0;
                    while (true)
                    {
                        generationAttempt++;
                        if (generationAttempt == 1)
                        {
                            //LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Starting label files generation attempt for label {formattedCurrentSequence}."));
                        }
                        else
                        {
                            LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Retrying label files generation attempt for label {formattedCurrentSequence}. Attempt {generationAttempt}."));
                        }

                        var generationLabelFilesResult = await GenerateLabelFilesAsync(job, sequence, cancellationToken);
                        if (generationLabelFilesResult.Result == StepResult.Success)
                        {
                            if (generationAttempt == 1)
                            {
                                //LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Label files generated successfully for label {formattedCurrentSequence}."));
                            }
                            else
                            {
                                //LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Label files generated successfully for label {formattedCurrentSequence} after {generationAttempt} attempt(s)."));
                            }

                            prnFileLocation = generationLabelFilesResult.PrnPath;

                            break;
                        }
                        if (generationLabelFilesResult.Result == StepResult.Retry)
                        {
                            continue;
                        }
                        if (generationLabelFilesResult.Result == StepResult.Stop)
                        {
                            LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Error, $"Job {jobId} stopped due to label files generation failure after {generationAttempt} attempt(s)."));

                            _jobController.Stop(jobId);
                            NotifyJobStateChanged(jobId);

                            _jobController.Ready(jobId);
                            NotifyJobStateChanged(jobId);
                            return;
                        }
                        break;
                    }


                    // PRINT
                    var printAttempt = 0;
                    while (true)
                    {
                        printAttempt++;
                        if (printAttempt == 1)
                        {
                            //LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Starting print attempt for label {formattedCurrentSequence}."));
                        }
                        else
                        {
                            //LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Retrying print attempt for label {formattedCurrentSequence}. Attempt {printAttempt}."));
                        }
                        var validationPrintResult = await ValidatePrintAsync(job, sequence, prnFileLocation, cancellationToken);
                        if (validationPrintResult == StepResult.Success)
                        {
                            if (printAttempt == 1)
                            {
                                //LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Label {formattedCurrentSequence} printed successfully."));
                            }
                            else
                            {
                                //LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Label {formattedCurrentSequence} printed successfully after {printAttempt} attempt(s)."));
                            }
                            break;
                        }
                        if (validationPrintResult == StepResult.Retry)
                        {
                            continue;
                        }
                        if (validationPrintResult == StepResult.Stop)
                        {
                            //LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Error, $"Job {jobId} stopped due to print failure after {printAttempt} attempt(s)."));

                            _jobController.Stop(jobId);
                            NotifyJobStateChanged(jobId);

                            _jobController.Ready(jobId);
                            NotifyJobStateChanged(jobId);
                            return;
                        }
                        break;
                    }

                    //job.Status = PrintJobStatus.Printing;
                    //NotifyJobStateChanged(jobId);
                    //PrintLabel(job, formattedCurrentSequence, formattedLastSequence); //Creating PRN and Image

                    EnsureCanContinue(job);

                    // BARCODE VALIDATION
                    var barcodeAttempt = 0;
                    while (true)
                    {
                        barcodeAttempt++;
                        if (barcodeAttempt == 1)
                        {
                            //LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Starting barcode validation for label {formattedCurrentSequence}."));
                        }
                        else
                        {
                            //LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Retrying barcode validation for label {formattedCurrentSequence}. Attempt {barcodeAttempt}."));
                        }
                        var validationBarcodeResult = await ValidateBarcodeAsync(job, sequence, cancellationToken);
                        if (validationBarcodeResult == StepResult.Success)
                        {
                            if (barcodeAttempt == 1)
                            {
                                //LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Label {formattedCurrentSequence} barcode validation passed successfully."));
                            }
                            else
                            {
                                //LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Barcode validation passed for label {formattedCurrentSequence} after {barcodeAttempt} attempt(s)."));
                            }
                            break;
                        }
                        if (validationBarcodeResult == StepResult.Retry)
                        {
                            continue;
                        }
                        if (validationBarcodeResult == StepResult.Skip) // Add reason for skip?
                        {
                            //LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Warning, $"Label {formattedCurrentSequence} skipped due to barcode failure after {barcodeAttempt} attempt(s)."));
                            break;
                        }
                        if (validationBarcodeResult == StepResult.Stop)
                        {
                            //LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Error, $"Job {jobId} stopped due to barcode failure after {barcodeAttempt} attempt(s)."));

                            _jobController.Stop(jobId);
                            NotifyJobStateChanged(jobId);

                            _jobController.Ready(jobId);
                            NotifyJobStateChanged(jobId);
                            return;
                        }
                        break;
                    }

                    // OCR VALIDATION
                    var ocrAttempt = 0;
                    while (true)
                    {
                        ocrAttempt++;
                        if (ocrAttempt == 1)
                        {
                            //LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Starting OCR validation for label {formattedCurrentSequence}."));
                        }
                        else
                        {
                            LogGenerated?.Invoke(this,
                                new LogEventArgs(MessageType.Info,
                                $"Retrying OCR validation for label {formattedCurrentSequence}. Attempt {ocrAttempt}."));
                        }

                        var validationOcrResult = await ValidateOcrAsync(job, sequence, cancellationToken);
                        if (validationOcrResult == StepResult.Success)
                        {
                            if (ocrAttempt == 1)
                            {
                                //LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Label {formattedCurrentSequence} OCR validation passed successfully."));
                            }
                            else
                            {
                                //LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"OCR validation passed for label {formattedCurrentSequence} after {ocrAttempt} attempt(s)."));
                            }
                            break;
                        }
                        if (validationOcrResult == StepResult.Retry)
                        {
                            continue;
                        }
                        if (validationOcrResult == StepResult.Skip) // Add reason for skip?
                        {
                            //LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Warning, $"Label {formattedCurrentSequence} skipped due to OCR failure after {ocrAttempt} attempt(s)."));
                            break;
                        }
                        if (validationOcrResult == StepResult.Stop)
                        {
                            //LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Error, $"Job {jobId} stopped due to OCR failure after {ocrAttempt} attempt(s)."));

                            _jobController.Stop(jobId);
                            NotifyJobStateChanged(jobId);

                            _jobController.Ready(jobId);
                            NotifyJobStateChanged(jobId);
                            return;
                        }
                        break; //
                    }

                    // Save only AFTER successful validation
                    job.PrintedCount = sequence;
                    NotifyJobStateChanged(jobId);

                    //LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Label {formattedCurrentSequence} processing completed and state saved successfully."));

                    sequence++;
                }

                _jobController.Complete(jobId);
                NotifyJobStateChanged(jobId);
                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, "Batch printing completed."));
                _jobController.Ready(jobId);
                NotifyJobStateChanged(jobId);

            }
            catch (OperationCanceledException)
            {
                NotifyJobStateChanged(jobId);
            }
            catch
            {
                _jobController.Fail(jobId);
                NotifyJobStateChanged(jobId);
                throw;
            }
            finally
            {
                _niceLabelTemplateService.CloseTemplate();
            }
        }

        private async Task<StepResult> RequestOperatorDecisionAsync(OperatorDecisionRequiredEventArgs args, CancellationToken cancellationToken)
        {
            _decisionTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

            OperatorDecisionRequired?.Invoke(this, args);
            await Task.Delay(500, cancellationToken);

            using (cancellationToken.Register(() =>
            {
                _decisionTcs.TrySetResult(StepResult.Stop);
            }))
            {
                return await _decisionTcs.Task;
            }
        }

        private async Task<(StepResult Result, string PrnPath)> GenerateLabelFilesAsync(PrintJobState job, int sequence, CancellationToken cancellationToken)
        {
            try
            {
                var formattedCurrentSequence = SequenceFormatter.Format(sequence, 5);

                var labelData = job.LabelData.Clone().Add("BOX_NO", formattedCurrentSequence);

                var templateVariables = _niceLabelTemplateService.GetTemplateVariables().ToHashSet(StringComparer.OrdinalIgnoreCase);

                var variablesToSet = labelData.Variables.Where(x => templateVariables.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);

                _niceLabelTemplateService.SetVariables(variablesToSet);

                var filename = PrintFileNameBuilder.Build(job.ItemCode, job.LotNo, formattedCurrentSequence);

                //LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Generating preview: {filename}.jpg"));

                var isPreviewGenerated = _niceLabelTemplateService.GeneratePreview(job.Paths.LabelsDirectory, filename);

                if (!isPreviewGenerated)
                {
                    //LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Error, $"Failed to generate preview image for {filename}"));

                    return (await RequestOperatorDecisionAsync(
                        new OperatorDecisionRequiredEventArgs
                        {
                            FailureType = ValidationFailure.FileGenerationFailed,
                            SequenceNo = sequence,
                            CustomMessage = $"Preview generation failed for {filename}"
                        },
                        cancellationToken), string.Empty);
                }

                //LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Preview image generated: {filename}.jpg"));

                //LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Generating prn file: {filename}.prn"));

                var isPrnGenerated = _niceLabelTemplateService.GeneratePrn(job.Paths.LabelsDirectory, filename, out string prnPath);

                if (!isPrnGenerated || string.IsNullOrWhiteSpace(prnPath) || !File.Exists(prnPath))
                {
                    //LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Error, $"Failed to generate PRN file for {filename}"));

                    return (await RequestOperatorDecisionAsync(
                        new OperatorDecisionRequiredEventArgs
                        {
                            FailureType = ValidationFailure.FileGenerationFailed,
                            SequenceNo = sequence,
                            CustomMessage = $"PRN generation failed for {filename}"
                        },
                        cancellationToken), string.Empty);
                }
                //LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"PRN file generated successfully: {prnPath}"));

                return (StepResult.Success, prnPath);

            }
            catch (Exception ex)
            {
                //LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Error, $"Fatal error generating label files: {ex.Message}"));

                return (await RequestOperatorDecisionAsync(
                    new OperatorDecisionRequiredEventArgs
                    {
                        FailureType = ValidationFailure.FileGenerationFailed,
                        SequenceNo = sequence,
                        CustomMessage = ex.Message
                    },
                    cancellationToken), string.Empty);
            }

        }
        private async Task<StepResult> ValidatePrintAsync(PrintJobState job, int sequence, string prnFileLocation, CancellationToken cancellationToken)
        {
            var formattedCurrentSequence = SequenceFormatter.Format(sequence, 5);
            var formattedLastSequence = SequenceFormatter.Format(job.LastSequenceToPrint, 5);

            EnsureCanContinue(job);

            job.Status = PrintJobStatus.Printing;
            NotifyJobStateChanged(job.JobId);


            var isPrinted = await _deviceManager.Printer.IsPrinted(prnFileLocation);
            //LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Printing label {formattedCurrentSequence}/{formattedLastSequence}"));
            if (isPrinted)
            {
                return StepResult.Success;
            }

            return await RequestOperatorDecisionAsync(
                            new OperatorDecisionRequiredEventArgs
                            {
                                FailureType = ValidationFailure.PrinterUnavailable,
                                SequenceNo = sequence,
                                CustomMessage = $"Printer is unavailable for label {formattedCurrentSequence}."
                            }, cancellationToken);
        }

        private async Task<StepResult> ValidateBarcodeAsync(PrintJobState job, int sequence, CancellationToken cancellationToken)
        {
            var formattedCurrentSequence = SequenceFormatter.Format(sequence, 5);
            var formattedLastSequence = SequenceFormatter.Format(job.LastSequenceToPrint, 5);

            EnsureCanContinue(job);
            return StepResult.Success;

            return await RequestOperatorDecisionAsync(
                            new OperatorDecisionRequiredEventArgs
                            {
                                FailureType = ValidationFailure.BarcodeMismatch,
                                SequenceNo = sequence,
                                CustomMessage = $"Barcode validation failed on label {formattedCurrentSequence}."
                            }, cancellationToken);

        }


        private async Task<StepResult> ValidateOcrAsync(PrintJobState job, int sequence, CancellationToken cancellationToken)
        {
            var formattedCurrentSequence = SequenceFormatter.Format(sequence, 5);
            var formattedLastSequence = SequenceFormatter.Format(job.LastSequenceToPrint, 5);

            EnsureCanContinue(job);

            return StepResult.Success; //uncomment for real implementation

            var result = await RequestOperatorDecisionAsync(
                         new OperatorDecisionRequiredEventArgs
                         {
                             FailureType = ValidationFailure.OcrMismatch,
                             SequenceNo = sequence,
                             CustomMessage = $"OCR validation failed on label {formattedCurrentSequence}."
                         }, cancellationToken);


            return result;
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
        public void Stop(Guid jobId)
        {
            _jobController.Stop(jobId);
            NotifyJobStateChanged(jobId);

            _decisionTcs?.TrySetResult(StepResult.Stop);

            // Can start print again
            _jobController.Ready(jobId);
            NotifyJobStateChanged(jobId);
        }

        public void SetUserDecision(StepResult decision)
        {
            _decisionTcs?.TrySetResult(decision);
        }


    }
}
