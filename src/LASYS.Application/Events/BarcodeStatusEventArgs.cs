using LASYS.Application.Common.Enums;

namespace LASYS.Application.Events
{
    public class BarcodeStatusEventArgs : EventArgs
    {
        public BarcodeStatus Status { get; }
        public string Message { get; }
        public string Description { get; }
        public BarcodeStatusEventArgs(BarcodeStatus status, string? description = null)
        {
            Status = status;
            var info = GetStatusInfo(status);
            Message = info.Message;
            Description = description ?? info.Description;
        }
        private (string Message, string Description) GetStatusInfo(BarcodeStatus status) => status switch
        {
            BarcodeStatus.BarcodeNotConfigured => ("Not Configured", "The barcode scanner has not been configured in the application settings."),
            BarcodeStatus.BarcodeNotDetected => ("Not Detected", "No barcode scanner device was found using the configured device name."),
            BarcodeStatus.BarcodeConnected => ("Connected", "The barcode scanner is connected and ready."),
            BarcodeStatus.BarcodeReconnecting => ("Reconnecting...", "The system is attempting to reconnect to the barcode scanner."),
            BarcodeStatus.BarcodeScanning => ("Scanning...", "The system is waiting for a barcode to be scanned."),
            BarcodeStatus.BarcodeScanned => ("Scan Received", "A barcode was successfully scanned."),
            BarcodeStatus.BarcodeCommunicating => ("Communicating...", "The system is sending a command to the barcode scanner."),
            BarcodeStatus.BarcodeDisconnected => ("Disconnected", "The barcode scanner is not connected."),
            BarcodeStatus.BarcodeTimeout => ("Timeout", "The barcode scanner did not respond within the expected time."),
            BarcodeStatus.BarcodeError => ("Error", "An error occurred while communicating with the barcode scanner."),
            _ => ("Unknown Status", "The barcode scanner status is unknown.")
        };
    }
}
