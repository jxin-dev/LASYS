namespace LASYS.DesktopApp.Events
{
    public class LabelPrintingRequestedEventArgs : EventArgs
    {
        public int WorkOrderId { get; }

        public LabelPrintingRequestedEventArgs(int workOrderId)
        {
            WorkOrderId = workOrderId;
        }
    }
}
