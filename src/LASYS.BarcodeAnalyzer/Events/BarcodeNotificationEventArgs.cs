using LASYS.BarcodeAnalyzer.Models;

namespace LASYS.BarcodeAnalyzer.Events
{
    public class BarcodeNotificationEventArgs : EventArgs
    {
        public BarcodeMessageType MessageType { get; }
        public string Message { get; }
        public BarcodeNotificationEventArgs(BarcodeMessageType messageType, string message)
        {
            MessageType = messageType;
            Message = message;
        }
    }
}
