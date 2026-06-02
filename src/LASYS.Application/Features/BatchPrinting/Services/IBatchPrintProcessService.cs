using LASYS.Application.Events;
using LASYS.Application.Features.BatchPrinting.Enums;
using LASYS.Application.Features.BatchPrinting.Events;
using LASYS.Application.Features.BatchPrinting.Models;
using LASYS.Application.Features.LabelInstructions.GetLabelInstructionContext;

namespace LASYS.Application.Features.BatchPrinting.Services
{
    public interface IBatchPrintProcessService
    {
        PrintJobState? GetJob(Guid jobId);
        event EventHandler<PrintJobState> JobStateChanged;
        Task<Guid> StartAsync(LabelPrintingContext context, int quantity);
        void Pause(Guid jobId);
        void Resume(Guid jobId);
        void Stop(Guid jobId);

        event EventHandler<OperatorDecisionRequiredEventArgs> OperatorDecisionRequired;
        void SetUserDecision(StepResult decision);
        event EventHandler<LogEventArgs> LogGenerated;
    }
}
