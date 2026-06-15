using LASYS.Application.Features.LabelInstructions.GetWorkOrderListBySectionId;
using LASYS.DesktopApp.Events;

namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface IWorkOrdersView
    {
        event EventHandler<LabelPrintingRequestedEventArgs> LabelPrintingRequested;
        void ShowNotification(string message, string caption, MessageBoxIcon icon);
        void SetLoading(bool isLoading);
        void SetWorkOrders(List<WorkOrderItem> labelInstructions, int totalCount);
        void InvokeOnUI(Action action);
    }
}
