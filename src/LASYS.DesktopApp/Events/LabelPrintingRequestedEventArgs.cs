using LASYS.Application.Features.LabelInstructions.GetWorkOrderListBySectionId;

namespace LASYS.DesktopApp.Events
{
    public class LabelPrintingRequestedEventArgs : EventArgs
    {
        public WorkOrderItem WorkOrderItem { get; }
        public LabelPrintingRequestedEventArgs(WorkOrderItem workOrderItem)
        {
            WorkOrderItem = workOrderItem;
        }
    }
}
