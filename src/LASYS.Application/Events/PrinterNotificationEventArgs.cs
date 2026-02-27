using LASYS.Application.Common.Enums;

namespace LASYS.Application.Events
{
    public class PrinterNotificationEventArgs : EventArgs
    {
        public MessageType MessageType { get; }
        public string Message { get; }
        public PrinterNotificationEventArgs(MessageType messageType, string message)
        {
            MessageType = messageType;
            Message = message;
        }
    }
}
