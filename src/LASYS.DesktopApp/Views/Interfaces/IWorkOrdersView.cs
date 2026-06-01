using LASYS.Application.Features.WorkOrders.GetWorkOrdersBySectionId;
using LASYS.DesktopApp.Events;

namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface IWorkOrdersView
    {
        event EventHandler<LabelPrintingRequestedEventArgs> LabelPrintingRequested;
        void ShowNotification(string message, string caption, MessageBoxIcon icon);
        void SetLoading(bool isLoading);
        void SetWorkOrders(List<Application.Features.LabelInstructions.GetWorkOrderListBySectionId.WorkOrderItem> labelInstructions, int totalCount);

    }
}
