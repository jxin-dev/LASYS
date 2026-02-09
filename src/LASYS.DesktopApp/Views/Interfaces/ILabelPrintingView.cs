using LASYS.DesktopApp.DTOs;
using LASYS.DesktopApp.Events;
using LASYS.DesktopApp.Views.Forms;

namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface ILabelPrintingView
    {
        event EventHandler BackToWorkOrdersRequested;
        event EventHandler PrintRequested;
        event EventHandler PausePrintingRequested;
        event EventHandler ResumePrintingRequested;
        event EventHandler StopPrintingRequested;

        event EventHandler<LabelOperationFailedEventArgs> LabelOperationFailed;
        void ShowError(ErrorForm errorForm);

        void UpdateWorkOrderData(WorkOrderDto workOrderDto);
        void UpdateProgress(int percent, string message);
        void HideError();
    }

}
