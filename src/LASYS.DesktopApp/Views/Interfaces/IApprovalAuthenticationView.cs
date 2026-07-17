using LASYS.Application.Common.Results;
using LASYS.Application.Features.BatchPrinting.Events;

namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface IApprovalAuthenticationView
    {
        void InvokeOnUI(Action action);

        event EventHandler<ApprovalCredentialsEventArgs> ApprovalRequested;
        event EventHandler ApprovalCancelled;
        void ApprovalSucceeded();
        void ApprovalFailed(string errorMessage);
        void CloseApproval();
    }
}
