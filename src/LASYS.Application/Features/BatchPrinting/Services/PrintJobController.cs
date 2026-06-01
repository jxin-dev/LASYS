using System.Collections.Concurrent;
using LASYS.Application.Common.Mappings;
using LASYS.Application.Features.BatchPrinting.Enums;
using LASYS.Application.Features.BatchPrinting.Helpers;
using LASYS.Application.Features.BatchPrinting.Models;
using LASYS.Application.Features.LabelInstructions.GetLabelInstructionContext;

namespace LASYS.Application.Features.BatchPrinting.Services
{
    public sealed class PrintJobController : IPrintJobController
    {
        private readonly ConcurrentDictionary<Guid, PrintJobState> _jobs = new();
        
        public Guid CreateJob(LabelPrintingContext context, int quantity)
        {
            var labelInstruction = context.LabelInstructionDetails!;
            var productDetails = context.ProductDetails!;
            var printDetails = context.PrintDetails!;
            var masterLabelDetails = context.MasterLabelDetails!;

            var paths = PrintJobPathBuilder.Create(labelInstruction.ItemCode, labelInstruction.LotNo, masterLabelDetails.BoxType.ToString());

            var jobId = Guid.NewGuid();

            var job = new PrintJobState
            {
                JobId = jobId,
                ItemCode = labelInstruction.ItemCode,
                LotNo = labelInstruction.LotNo,
                BoxType = masterLabelDetails.BoxType.ToString(),
                NiceLabelFilePath = masterLabelDetails.FilePath!,
                TotalQuantity = quantity,
                PrintedCount = 0,
                StartSequenceToPrint = (int)printDetails.NextSequence,
                Status = PrintJobStatus.InProgress,
                LabelData = NiceLabelDataMappings.ToLabelData(context),
                Paths = paths
            };

            _jobs.TryAdd(jobId, job);

            return jobId;
        }
        public void Complete(Guid jobId)
        {
            var job = GetJob(jobId);
            if (job is not null)
                job.Status = PrintJobStatus.Completed;
        }
        public void Fail(Guid jobId)
        {
            var job = GetJob(jobId);
            if (job is not null)
                job.Status = PrintJobStatus.Failed;
        }

        public PrintJobState? GetJob(Guid jobId)
        {
            _jobs.TryGetValue(jobId, out var job);
            return job;
        }
        public void Ready(Guid jobId)
        {
            var job = GetJob(jobId);
            if (job is not null)
                job.Status = PrintJobStatus.Ready;
        }
        public void Pause(Guid jobId)
        {
            var job = GetJob(jobId);
            if (job is null) return;

            if (job.Status != PrintJobStatus.Printing)
                return;

            job.Status = PrintJobStatus.Paused;
            job.ResumeSignal.Reset();
        }

        public void Resume(Guid jobId)
        {
            var job = GetJob(jobId);
            if (job is null) return;

            if (job.Status != PrintJobStatus.Paused)
                return;

            job.Status = PrintJobStatus.Printing;
            job.ResumeSignal.Set();
        }

        public void Stop(Guid jobId)
        {
            var job = GetJob(jobId);
            if (job is null) return;

            if (job.Status == PrintJobStatus.Stopped)
                return;

            job.Status = PrintJobStatus.Stopped;
            job.CancellationTokenSource.Cancel();

            // wake any paused thread
            job.ResumeSignal.Set();
        }       
    }
}
