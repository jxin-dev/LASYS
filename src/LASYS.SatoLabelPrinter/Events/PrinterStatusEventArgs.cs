namespace LASYS.SatoLabelPrinter.Events
{
    public class PrinterStatusEventArgs : EventArgs
    {
        public PrinterStatus Status { get; }
        public string Message { get; }
        public PrinterStatusEventArgs(PrinterStatus status, string? message = null)
        {
            Status = status;
            Message = message ?? GetDefaultMessage(status); ;
        }

        private string GetDefaultMessage(PrinterStatus status) => status switch
        {
            PrinterStatus.Ready => "Printer is ready.",
            PrinterStatus.Offline => "Printer is offline.",
            PrinterStatus.Printing => "Printer is printing.",
            PrinterStatus.Paused => "Printer is paused.",
            PrinterStatus.Error => "Printer error occurred.",
            _ => "Unknown printer status."
        };
    }
    public enum PrinterStatus
    {
        Ready,
        Offline,
        Printing,
        Paused,
        Error,
    }
}
