namespace LASYS.OCR.Events
{
    public sealed class OCRCompletedEventArgs : EventArgs
    {
        public string Result { get; }
        public bool Success { get; }
        public string Message => Success
            ? $"OCR succeeded. Text length: {Result?.Length ?? 0} characters."
            : "OCR failed or no text detected.";

        public OCRCompletedEventArgs(string result, bool success)
        {
            Result = result;
            Success = success;
        }
    }
}
