using LASYS.Application.Common.Messaging;
using LASYS.Application.Common.Utilities;
using LASYS.Application.Events;
using LASYS.Application.Features.BatchPrinting.Enums;
using LASYS.Application.Features.BatchPrinting.Events;
using LASYS.Application.Features.BatchPrinting.Helpers;
using LASYS.Application.Features.BatchPrinting.Models;
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
        private readonly IPrinterService _printerService;
        private readonly IBarcodeService _barcodeService;
        private readonly IOCRService _ocrService;

        private TaskCompletionSource<StepResult>? _decisionTcs;
        public event EventHandler<OperatorDecisionRequiredEventArgs>? OperatorDecisionRequired;
        public event EventHandler<PrintJobState>? JobStateChanged;
        public event EventHandler<LogEventArgs>? LogGenerated;

        public BatchPrintProcessService(ICurrentUser currentUser, IPrintJobController jobController, INiceLabelTemplateService niceLabelTemplateService, IPrinterService printerService, IBarcodeService barcodeService, IOCRService ocrService)
        {
            _currentUser = currentUser;
            _jobController = jobController;
            _niceLabelTemplateService = niceLabelTemplateService;
            _printerService = printerService;
            _barcodeService = barcodeService;
            _ocrService = ocrService;
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
                await _printerService.InitializeAsync();

                var isTemplateLoaded = _niceLabelTemplateService.IsTemplateLoaded;
                if (!isTemplateLoaded)
                {
                    _niceLabelTemplateService.LoadTemplate(job.NiceLabelFilePath);
                }


                var sequence = job.StartSequenceToPrint;

                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, $"Started print job. Total quantity: {job.TotalQuantity}"));

                while (sequence <= job.TotalQuantity)
                {
                    EnsureCanContinue(job);

                    var formattedCurrentSequence = SequenceFormatter.Format(sequence, 5);
                    var formattedLastSequence = SequenceFormatter.Format(job.LastSequenceToPrint, 5);

                    // PRINT
                    job.Status = PrintJobStatus.Printing;
                    NotifyJobStateChanged(jobId);
                    PrintLabel(job, formattedCurrentSequence, formattedLastSequence); //Creating PRN and Image

                    EnsureCanContinue(job);

                    // BARCODE VALIDATION
                    var barcodeAttempt = 0;
                    while (true)
                    {
                        barcodeAttempt++;
                        if (barcodeAttempt == 1)
                        {
                            LogGenerated?.Invoke(this,
                                new LogEventArgs(MessageType.Info,
                                $"Starting barcode validation for label {formattedCurrentSequence}."));
                        }
                        else
                        {
                            LogGenerated?.Invoke(this,
                                new LogEventArgs(MessageType.Info,
                                $"Retrying barcode validation for label {formattedCurrentSequence}. Attempt {barcodeAttempt}."));
                        }
                        var validationBarcodeResult = await ValidateBarcodeAsync(job, sequence, cancellationToken);
                        if (validationBarcodeResult == StepResult.Success)
                        {
                            if (barcodeAttempt == 1)
                            {
                                LogGenerated?.Invoke(this,
                                    new LogEventArgs(MessageType.Info,
                                    $"Label {formattedCurrentSequence} barcode validation passed successfully."));
                            }
                            else
                            {
                                LogGenerated?.Invoke(this,
                                    new LogEventArgs(MessageType.Info,
                                    $"Barcode validation passed for label {formattedCurrentSequence} after {barcodeAttempt} attempt(s)."));
                            }
                            break;
                        }
                        if (validationBarcodeResult == StepResult.Retry)
                        {
                            continue;
                        }
                        if (validationBarcodeResult == StepResult.Skip) // Add reason for skip?
                        {
                            LogGenerated?.Invoke(this,
                                new LogEventArgs(MessageType.Warning,
                                $"Label {formattedCurrentSequence} skipped due to barcode failure after {barcodeAttempt} attempt(s)."));
                            break;
                        }
                        if (validationBarcodeResult == StepResult.Stop)
                        {
                            LogGenerated?.Invoke(this,
                               new LogEventArgs(MessageType.Error,
                               $"Job {jobId} stopped due to barcode failure after {barcodeAttempt} attempt(s)."));

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
                            LogGenerated?.Invoke(this,
                                new LogEventArgs(MessageType.Info,
                                $"Starting OCR validation for label {formattedCurrentSequence}."));
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
                                LogGenerated?.Invoke(this,
                                    new LogEventArgs(MessageType.Info,
                                    $"Label {formattedCurrentSequence} OCR validation passed successfully."));
                            }
                            else
                            {
                                LogGenerated?.Invoke(this,
                                        new LogEventArgs(MessageType.Info,
                                        $"OCR validation passed for label {formattedCurrentSequence} after {ocrAttempt} attempt(s)."));
                            }
                            break;
                        }
                        if (validationOcrResult == StepResult.Retry)
                        {
                            continue;
                        }
                        if (validationOcrResult == StepResult.Skip) // Add reason for skip?
                        {
                            LogGenerated?.Invoke(this,
                                new LogEventArgs(MessageType.Warning,
                                $"Label {formattedCurrentSequence} skipped due to OCR failure after {ocrAttempt} attempt(s)."));
                            break;
                        }
                        if (validationOcrResult == StepResult.Stop)
                        {
                            LogGenerated?.Invoke(this,
                               new LogEventArgs(MessageType.Error,
                               $"Job {jobId} stopped due to OCR failure after {ocrAttempt} attempt(s)."));

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

                    LogGenerated?.Invoke(this,
                        new LogEventArgs(MessageType.Info,
                        $"Label {formattedCurrentSequence} processing completed and state saved successfully."));

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

        private void PrintLabel(PrintJobState job, string currentSequence, string lastSequence)
        {
            var labelData = job.LabelData.Clone().Add("BOX_NO", currentSequence);

            var templateVariables = _niceLabelTemplateService.GetTemplateVariables().ToHashSet(StringComparer.OrdinalIgnoreCase);

            var variablesToSet = labelData.Variables.Where(x => templateVariables.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);

            _niceLabelTemplateService.SetVariables(variablesToSet);

            var filename = PrintFileNameBuilder.Build(job.ItemCode, job.LotNo, currentSequence);
            LogGenerated?.Invoke(this,
                new LogEventArgs(MessageType.Info, $"Generating preview: {filename}.jpg"));

            var isPreviewGenerated = _niceLabelTemplateService.GeneratePreview(
                outputDirectory: job.Paths.LabelsDirectory,
                fileName: filename);

            if (!isPreviewGenerated)
            {
                LogGenerated?.Invoke(this,
                    new LogEventArgs(MessageType.Error, $"Failed to generate preview image for {filename}"));
            }

            LogGenerated?.Invoke(this,
                new LogEventArgs(MessageType.Info, $"Preview image generated: {filename}.jpg"));

            LogGenerated?.Invoke(this,
               new LogEventArgs(MessageType.Info, $"Generating prn file: {filename}.prn"));

            var isPrnGenerated = _niceLabelTemplateService.GeneratePrn(
                outputDirectory: job.Paths.LabelsDirectory,
                fileName: filename,
                out var prnPath);

            if (!isPrnGenerated || string.IsNullOrWhiteSpace(prnPath) || !File.Exists(prnPath))
            {
                LogGenerated?.Invoke(this,
                    new LogEventArgs(MessageType.Error, $"Failed to generate PRN file for {filename}"));
            }
            LogGenerated?.Invoke(this,
                new LogEventArgs(MessageType.Info, $"PRN file generated successfully: {prnPath}"));

            if (isPreviewGenerated && isPrnGenerated)
            {
                _printerService.Print(prnPath);
                LogGenerated?.Invoke(this,
                new LogEventArgs(MessageType.Info, $"Printing label {currentSequence}/{lastSequence}"));
            }
        }

        private async Task<StepResult> RequestOperatorDecisionAsync(OperatorDecisionRequiredEventArgs args, CancellationToken cancellationToken)
        {
            _decisionTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

            OperatorDecisionRequired?.Invoke(this, args);

            using (cancellationToken.Register(() =>
            {
                _decisionTcs.TrySetResult(StepResult.Stop);
            }))
            {
                return await _decisionTcs.Task;
            }
        }

        private async Task<StepResult> ValidateBarcodeAsync(PrintJobState job, int sequence, CancellationToken cancellationToken)
        {
            EnsureCanContinue(job);
            await Task.Delay(300, cancellationToken);

            var result = await RequestOperatorDecisionAsync(
                            new OperatorDecisionRequiredEventArgs
                            {
                                FailureType = ValidationFailure.BarcodeMismatch,
                                SequenceNo = sequence,
                                CustomMessage = $"Barcode validation failed on label {sequence}."
                            }, cancellationToken);

            if (result == StepResult.Success)
                return StepResult.Success;

            return result; //uncomment for real implementation
        }


        private async Task<StepResult> ValidateOcrAsync(PrintJobState job, int sequence, CancellationToken cancellationToken)
        {
            EnsureCanContinue(job);
            await Task.Delay(300, cancellationToken);

            var result = await RequestOperatorDecisionAsync(
                         new OperatorDecisionRequiredEventArgs
                         {
                             FailureType = ValidationFailure.OcrMismatch,
                             SequenceNo = sequence,
                             CustomMessage = $"OCR validation failed on label {sequence}."
                         }, cancellationToken);

            if (result == StepResult.Success)
                return StepResult.Success;

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
