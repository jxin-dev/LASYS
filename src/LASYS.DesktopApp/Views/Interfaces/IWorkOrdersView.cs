using LASYS.Application.Features.LabelProcessing.GetWorkOrders;
using LASYS.DesktopApp.Events;
using LASYS.DesktopApp.Views.UserControls;

namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface IWorkOrdersView
    {
        event EventHandler<LabelPrintingRequestedEventArgs> LabelPrintingRequested;
        event EventHandler<int> PageNoChanged;
        event EventHandler<string> SearchTermChanged;

        void ShowNotification(string message, string caption, MessageBoxIcon icon);
        void SetLoading(bool isLoading);
        void SetWorkOrders(List<GetWorkOrdersResult> workOrders, int totalCount);
    }
}
