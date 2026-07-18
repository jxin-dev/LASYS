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
        Task<PrintJobState> InitializeAsync(LabelPrintingContext context);
        Task StartAsync(Guid jobId, int quantity, bool endOfBatch);
        void Pause(Guid jobId);
        void Resume(Guid jobId);
        void Stop(Guid jobId);

        event EventHandler<OperatorDecisionRequiredEventArgs> OperatorDecisionRequired;

        event EventHandler ApprovalAuthorizationRequired;
        Task<ApprovalAuthorizationResult> RequestApprovalAsync(CancellationToken cancellationToken = default);
        //Task<ApprovalAuthorizationResult> RequestApprovalIfPrintedAsync(CancellationToken cancellationToken = default);

        void SetApprovalAuthorized(string userCode, string sectionId);
        void CancelApprovalAuthorization();
        void SetUserDecision(StepResult decision);
        event EventHandler<LogEventArgs> LogGenerated;
    }
}
