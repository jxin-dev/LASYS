using LASYS.Application.Common.Messaging;
using LASYS.Application.Features.BatchPrinting.Enums;
using LASYS.Application.Features.BatchPrinting.Events;
using LASYS.Application.Features.BatchPrinting.Models;
using LASYS.Application.Features.Devices.Models;
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

        event EventHandler CameraPreviewRequested;
        event EventHandler LabelTemplatePreviewRequested;
        //void SetCameraPreview(UserControl control);
        void ToggleCameraPreview(bool visible);

        void SetPreview(UserControl control);
        void ToggleLabelTemplatePreview(bool visible);

        void SetPrintingState(PrintJobStatus status);
        void UpdateProgress(int printedCount, int totalQuantity);
        void UpdateQuantityControl(PrintJobState printJob);
        void UpdatePrintingResults(uint targetQuantity, long setNumber, long batchNumber, long startSequence, long remaining, long totalPrinted, long totalPassed, long totalFailed, long labelSample);
        void ToggleActivityLogs();
        void ShowError(ErrorForm errorForm);
        void InvokeOnUI(Action action);
        void AddLog(MessageType type, DateTime timeStamp, string message);
        void InitializePrintingContext(PrintJobState printJob);
        void HideError();
        //void UpdateCameraStatus(string status, string details);
        //void UpdatePrinterStatus(string status, string details);
        //void UpdateBarcodeStatus(string status, string details);
        void UpdateDeviceStatus(DeviceStatus status);

    }

}
