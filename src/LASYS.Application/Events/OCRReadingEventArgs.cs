namespace LASYS.Application.Events
{
    public sealed class OCRReadingEventArgs : EventArgs
    {
        public bool IsReading { get; }
        public OCRReadingEventArgs(bool isReading)
        {
            IsReading = isReading;
        }
    }
}
