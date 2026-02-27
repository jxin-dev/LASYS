namespace LASYS.Application.Events
{
    public class BarcodeStatusEventArgs : EventArgs
    {
        public string Message { get; }
        public bool IsError { get; }
        public BarcodeStatusEventArgs(string message, bool isError = false)
        {
            Message = message;
            IsError = isError;
        }
    }
}
