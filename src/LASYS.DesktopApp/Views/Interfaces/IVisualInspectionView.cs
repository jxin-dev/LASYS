using LASYS.DesktopApp.Events;

namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface IVisualInspectionView
    {
        event EventHandler<VisualInspectionApprovalEventArgs> ApprovalRequested;
        void HideInspection();
        void ShowInspection();
        void CloseInspection();
        void InvokeOnUI(Action action);

    }
}
