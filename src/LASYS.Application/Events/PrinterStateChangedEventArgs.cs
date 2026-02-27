using LASYS.Application.Common.Enums;

namespace LASYS.Application.Events
{
    public class PrinterStateChangedEventArgs : EventArgs
    {
        public PrinterStatus Status { get; }
        public string Message { get; }
        public PrinterStateChangedEventArgs(PrinterStatus status, string? message = null)
        {
            Status = status;
            Message = message ?? GetDefaultMessage(status);
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
    }
