using LASYS.Application.Common.Messaging;
using LASYS.Application.Features.BatchPrinting.Enums;
using LASYS.Application.Features.BatchPrinting.Events;
using LASYS.Application.Features.Devices.Enums;
using LASYS.Application.Features.Devices.Models;
using LASYS.Application.Features.LabelInstructions.GetLabelInstructionContext;
using LASYS.DesktopApp.Views.Forms;

namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface ILabelPrintingView
    {
        event EventHandler BackToWorkOrdersRequested;

        event EventHandler<PrintRequestedEventArgs> PrintRequested;
        event EventHandler PausePrintingRequested;
        event EventHandler ResumePrintingRequested;
        event EventHandler StopPrintingRequested;

        void SetPrintingState(PrintJobStatus status);
        void UpdateProgress(int printedCount, int totalQuantity);
        void ToggleActivityLogs();
        void ShowError(ErrorForm errorForm);
        void InvokeOnUI(Action action);
        void AddLog(MessageType type, DateTime timeStamp, string message);
        void LoadPrintingContext(LabelPrintingContext context);
        void HideError();
        //void UpdateCameraStatus(string status, string details);
        //void UpdatePrinterStatus(string status, string details);
        //void UpdateBarcodeStatus(string status, string details);
        void UpdateDeviceStatus(DeviceStatus status);
    }

}
