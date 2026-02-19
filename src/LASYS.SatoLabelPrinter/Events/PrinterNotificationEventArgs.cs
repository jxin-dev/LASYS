using LASYS.SatoLabelPrinter.Models;

namespace LASYS.SatoLabelPrinter.Events
{
    public class PrinterNotificationEventArgs : EventArgs
    {
        public PrinterMessageType MessageType { get; }
        public string Message { get; }
        public PrinterNotificationEventArgs(PrinterMessageType messageType, string message)
        {
            MessageType = messageType;
            Message = message;
        }
    }
}
