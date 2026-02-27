namespace LASYS.Application.Events
{
    public class BarcodeScannedEventArgs : EventArgs
    {
        public string Value { get; }
        public DateTime Timestamp { get; }

        public BarcodeScannedEventArgs(string value)
        {
            Value = value;
            Timestamp = DateTime.UtcNow;
        }

    }
}
