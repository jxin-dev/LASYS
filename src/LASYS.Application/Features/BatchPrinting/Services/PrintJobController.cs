using System.Collections.Concurrent;
using LASYS.Application.Features.BatchPrinting.Enums;
using LASYS.Application.Features.BatchPrinting.Models;
using LASYS.Application.Features.LabelInstructions.GetLabelInstructionContext;

namespace LASYS.Application.Features.BatchPrinting.Services
{
    public sealed class PrintJobController : IPrintJobController
    {
        private readonly ConcurrentDictionary<Guid, PrintJobState> _jobs = new();

        public Guid CreateJob(string printerName, LabelPrintingContext context)
        {
            var job = PrintJobState.Create(printerName, context);

            _jobs.TryAdd(job.JobId, job);

            return job.JobId;
        }
        public void Complete(Guid jobId)
        {
            var job = GetJob(jobId);
            if (job is not null)
                job.Completed();
        }
    
        public PrintJobState? GetJob(Guid jobId)
        {
            _jobs.TryGetValue(jobId, out var job);
            return job;
        }
        public void Reset(Guid jobId)
        {
            var job = GetJob(jobId);
            if (job is null) return;

            job.Reset();
        }
        public void Pause(Guid jobId)
        {
            var job = GetJob(jobId);
            if (job is null) return;

            if (job.Status != PrintJobStatus.InProgress)
                return;

            job.Paused();
        }

        public void Resume(Guid jobId)
        {
            var job = GetJob(jobId);
            if (job is null) return;

            if (job.Status != PrintJobStatus.Paused)
                return;

            job.InProgress();
        }

        public void Stop(Guid jobId, bool hasApproval = false)
        {
            var job = GetJob(jobId);
            if (job is null) return;

            if (job.Status == PrintJobStatus.Stopped)
                return;

            job.Stopped(hasApproval);
        }

        public void Printed(Guid jobId)
        {
            var job = GetJob(jobId);
            if (job is null) return;

            if (job.Status == PrintJobStatus.Printed)
                return;

            job.Printed();
        }
    }
}

