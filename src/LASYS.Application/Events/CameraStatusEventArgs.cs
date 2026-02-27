namespace LASYS.Application.Events
{
    public sealed class CameraStatusEventArgs : EventArgs
    {
        public string StatusMessage { get; }
        public bool IsError { get; }
        public CameraStatusEventArgs(string statusMessage, bool isError = false)
        {
            StatusMessage = statusMessage;
            IsError = isError;
        }
    }
}
