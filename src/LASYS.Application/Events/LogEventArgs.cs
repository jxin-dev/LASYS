using LASYS.Application.Common.Messaging;

namespace LASYS.Application.Events
{
    public class LogEventArgs : EventArgs
    {
        public MessageType Type { get; }
        public string Message { get; }
        public DateTime Timestamp { get; }

        public LogEventArgs(MessageType type, string message)
        {
            Type = type;
            Message = message;
            Timestamp = DateTime.Now;
        }
    }
}
