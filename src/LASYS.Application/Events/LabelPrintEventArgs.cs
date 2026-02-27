namespace LASYS.Application.Events
{
    public class LabelPrintEventArgs : EventArgs
    {
        public string Message { get; }
        public bool IsError { get; }
        public LabelPrintEventArgs(string message, bool isError = false)
        {
            Message = message;
            IsError = isError;
        }
    }
}
