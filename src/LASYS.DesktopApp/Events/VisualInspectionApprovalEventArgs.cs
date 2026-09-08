namespace LASYS.DesktopApp.Events
{
    public class VisualInspectionApprovalEventArgs : EventArgs
    {
        public bool IsRejection { get; }
        public VisualInspectionApprovalEventArgs(bool isRejection)
        {
            IsRejection = isRejection;
        }
    }
}
