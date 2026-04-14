using LASYS.DesktopApp.Events;
using LASYS.DesktopApp.Views.UserControls;

namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface IWorkOrdersView
    {
        event EventHandler<LabelPrintingRequestedEventArgs> LabelPrintingRequested;
        void ShowNotification(string message, string caption, MessageBoxIcon icon);
        void SetWorkOrders(List<SampleData> workOrders);
    }
}
