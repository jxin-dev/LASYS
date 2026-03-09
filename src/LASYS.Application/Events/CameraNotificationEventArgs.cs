namespace LASYS.Application.Events
{
    public sealed class CameraNotificationEventArgs : EventArgs
    {
        public string Message { get; }
        public string Caption { get; }
        public bool IsError { get; }
        public CameraNotificationEventArgs(string message, string caption, bool isError = false)
        {
            Message = message;
            Caption = caption;
            IsError = isError;
        }
    }
}
