using LASYS.Application.Common.Messaging;
using LASYS.Application.Common.Utilities;
using LASYS.Application.Events;
using LASYS.Application.Features.BatchPrinting.Enums;
using LASYS.Application.Features.BatchPrinting.Events;
using LASYS.Application.Features.BatchPrinting.Helpers;
using LASYS.Application.Features.BatchPrinting.Models;
using LASYS.Application.Features.LabelInstructions.GetLabelInstructionContext;
using LASYS.Application.Interfaces.Services;

namespace LASYS.Application.Features.BatchPrinting.Services
{
    public sealed class BatchPrintProcessService : IBatchPrintProcessService
    {
        private readonly IPrintJobController _jobController;
        private readonly INiceLabelTemplateService _niceLabelTemplateService;
        private readonly IPrinterService _printerService;

        private TaskCompletionSource<OperatorDecision>? _decisionTcs;

        public event EventHandler<OperatorDecisionRequiredEventArgs>? OperatorDecisionRequired;
        public event EventHandler<PrintJobState>? JobStateChanged;
        public event EventHandler<LogEventArgs>? LogGenerated;
        public BatchPrintProcessService(IPrintJobController jobController,
                                        INiceLabelTemplateService niceLabelTemplateService,
                                        IPrinterService printerService)
        {
            _jobController = jobController;
            _niceLabelTemplateService = niceLabelTemplateService;
            _printerService = printerService;
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

                    // PRINT
                    job.Status = PrintJobStatus.Printing;
                    NotifyJobStateChanged(jobId);
                    PrintLabel(job, sequence); //Creating PRN and Image

                    EnsureCanContinue(job);

                    // BARCODE VALIDATION
                    if (!await ValidateBarcodeAsync(jobId, sequence, cancellationToken))
                    {
                        var decision = await RequestOperatorDecisionAsync(
                            new OperatorDecisionRequiredEventArgs
                            {
                                FailureType = ValidationFailure.BarcodeMismatch,
                                SequenceNo = sequence,
                                CustomMessage = $"Barcode validation failed on label {sequence}."
                            }, cancellationToken);

                        if (decision == OperatorDecision.Retry)
                        {
                            continue; // retry same sequence
                        }

                        if (decision == OperatorDecision.Skip)
                        {
                            job.PrintedCount = sequence;
                            sequence++;
                            continue;
                        }

                        if (decision == OperatorDecision.Stop)
                        {
                            _jobController.Stop(jobId);
                            NotifyJobStateChanged(jobId);
                            return;
                        }
                    }
                    EnsureCanContinue(job);

                    // OCR VALIDATION
                    if (!await ValidateOcrAsync(jobId, sequence, cancellationToken))
                    {
                        var decision = await RequestOperatorDecisionAsync(
                           new OperatorDecisionRequiredEventArgs
                           {
                               FailureType = ValidationFailure.OcrMismatch,
                               SequenceNo = sequence,
                               CustomMessage = $"OCR validation failed on label {sequence}."
                           }, cancellationToken);

                        if (decision == OperatorDecision.Retry)
                        {
                            continue; // retry same sequence
                        }

                        if (decision == OperatorDecision.Skip)
                        {
                            job.PrintedCount = sequence;
                            sequence++;
                            continue;
                        }

                        if (decision == OperatorDecision.Stop)
                        {
                            _jobController.Stop(jobId);
                            NotifyJobStateChanged(jobId);
                            return;
                        }
                    }
                    EnsureCanContinue(job);

                    LogGenerated?.Invoke(this,
                        new LogEventArgs(MessageType.Info, $"Label {sequence} successfully printed and validated"));


                    // Save only AFTER successful validation
                    job.PrintedCount = sequence;
                    NotifyJobStateChanged(jobId);
                    Console.WriteLine($"Printed: {job.PrintedCount}/{job.TotalQuantity}");
                    sequence++;
                }

                _jobController.Complete(jobId);
                NotifyJobStateChanged(jobId);
                LogGenerated?.Invoke(this, new LogEventArgs(MessageType.Info, "Batch printing completed."));

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

        private void PrintLabel(PrintJobState job, int sequence)
        {
            var lastSequenceToPrint = SequenceFormatter.Format(job.LastSequenceToPrint, 5);
            var formattedSequence = SequenceFormatter.Format(sequence, 5);

            var labelData = job.LabelData.Clone().Add("BOX_NO", formattedSequence);

            var templateVariables = _niceLabelTemplateService.GetTemplateVariables().ToHashSet(StringComparer.OrdinalIgnoreCase);

            var variablesToSet = labelData.Variables.Where(x => templateVariables.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);

            _niceLabelTemplateService.SetVariables(variablesToSet);

            var filename = PrintFileNameBuilder.Build(job.ItemCode, job.LotNo, formattedSequence);
            LogGenerated?.Invoke(this,
                new LogEventArgs(MessageType.Info, $"Generating preview: {filename}.jpg"));

            var isPreviewGenerated = _niceLabelTemplateService.GeneratePreview(
                outputDirectory: job.Paths.LabelsDirectory,
                fileName: filename);

            LogGenerated?.Invoke(this,
               new LogEventArgs(MessageType.Info, $"Generating prn file: {filename}.prn"));
            var prnPath = _niceLabelTemplateService.GeneratePrn(
                outputDirectory: job.Paths.LabelsDirectory,
                fileName: filename);

            _printerService.Print(prnPath);
            LogGenerated?.Invoke(this,
                new LogEventArgs(MessageType.Info, $"Printing label {formattedSequence}/{lastSequenceToPrint}"));

        }

        private async Task<OperatorDecision> RequestOperatorDecisionAsync(OperatorDecisionRequiredEventArgs args, CancellationToken cancellationToken)
        {
            _decisionTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

            OperatorDecisionRequired?.Invoke(this, args);

            using (cancellationToken.Register(() =>
            {
                _decisionTcs.TrySetResult(OperatorDecision.Stop);
            }))
            {
                return await _decisionTcs.Task;
            }
        }

        private async Task<bool> ValidateBarcodeAsync(Guid jobId, int sequence, CancellationToken cancellationToken)
        {
            await Task.Delay(300, cancellationToken);

            return false;
        }

        private async Task<bool> ValidateOcrAsync(Guid jobId, int sequence, CancellationToken cancellationToken)
        {
            await Task.Delay(300, cancellationToken);

            return true;
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

            _decisionTcs?.TrySetResult(OperatorDecision.Stop);

            // Can start print again
            _jobController.Ready(jobId);
            NotifyJobStateChanged(jobId);


        }

        public void SetUserDecision(OperatorDecision decision)
        {
            _decisionTcs?.TrySetResult(decision);
        }


    }
}
