using LASYS.Application.Common.Enums;
namespace LASYS.Application.Events
{
    public class BarcodeNotificationEventArgs : EventArgs
    {
        public MessageType MessageType { get; }
        public string Message { get; }
        public BarcodeNotificationEventArgs(MessageType messageType, string message)
        {
            MessageType = messageType;
            Message = message;
        }
    }
}
