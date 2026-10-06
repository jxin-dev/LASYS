using LASYS.Application.Features.BatchPrinting.Enums;

namespace LASYS.DesktopApp.Events
{
    public class VisualInspectionApprovalEventArgs : EventArgs
    {
        public bool IsRejection { get; }
        public bool RequiresApproval { get; }
        public VisualInspectionApprovalEventArgs(bool isRejection, bool requiresApproval)
        {
            IsRejection = isRejection;
            RequiresApproval = requiresApproval;
        }
    }
}
