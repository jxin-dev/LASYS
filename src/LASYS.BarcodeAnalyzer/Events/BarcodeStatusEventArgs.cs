namespace LASYS.BarcodeAnalyzer.Events
{
    public class BarcodeStatusEventArgs : EventArgs
    {
        public string Message { get; }
        public BarcodeStatusEventArgs(string message)
        {
            Message = message;
        }
    }
}
