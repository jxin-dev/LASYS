using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Messaging;
using LASYS.Application.Common.Models;
using LASYS.DesktopApp.DTOs;
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
        void ShowError(ErrorForm errorForm);
        //void AddLog(StatusMessage message);
        void InvokeOnUI(Action action);
        void AddLog(MessageType type, DateTime timeStamp, string message);

        void SetPrintingState(PrintingState state);
        void UpdateWorkOrderData(WorkOrderDto workOrderDto);
        void UpdatePrintData(PrintData printData);

        void UpdateFilePath(string filePath);
        //void UpdateProgress(int percent, string message);
        void HideError();
        void UpdateCameraStatus(string status, string details);
        void UpdatePrinterStatus(string status, string details);
        void UpdateBarcodeStatus(string status, string details);

    }

}
