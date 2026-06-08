using System.Collections.Concurrent;
using System.Collections.Generic;
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
        
        public Guid CreateJob(string printerName, LabelPrintingContext context, int quantity)
        {
            var job = PrintJobState.Create(printerName, context, quantity);

            _jobs.TryAdd(job.JobId, job);

            return job.JobId;
        }
        public void Complete(Guid jobId)
        {
            var job = GetJob(jobId);
            if (job is not null)
                job.Completed();
        }
        public void Fail(Guid jobId)
        {
            var job = GetJob(jobId);
            if (job is not null)
                job.Failed();
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
                job.Ready();
        }
        public void Pause(Guid jobId)
        {
            var job = GetJob(jobId);
            if (job is null) return;

            if (job.Status != PrintJobStatus.InProgress)
                return;

            job.Paused();
            job.ResumeSignal.Reset();
        }

        public void Resume(Guid jobId)
        {
            var job = GetJob(jobId);
            if (job is null) return;

            if (job.Status != PrintJobStatus.Paused)
                return;

            job.InProgress();
            job.ResumeSignal.Set();
        }

        public void Stop(Guid jobId)
        {
            var job = GetJob(jobId);
            if (job is null) return;

            if (job.Status == PrintJobStatus.Stopped)
                return;

            job.Stopped();
            job.CancellationTokenSource.Cancel();

            // wake any paused thread
            job.ResumeSignal.Set();
        }       
    }
}
