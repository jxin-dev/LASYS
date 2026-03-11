using LASYS.Application.Common.Enums;

namespace LASYS.Application.Events
{
    public class PrinterStatusEventArgs : EventArgs
    {
        public PrinterStatus Status { get; }
        public string Message { get; }
        public string Description { get; }
        public PrinterStatusEventArgs(PrinterStatus status, string? description = null)
        {
            Status = status;
            var info = GetStatusInfo(status);
            Message = info.Message;
            Description = description ?? info.Description;
        }

        private (string Message, string Description) GetStatusInfo(PrinterStatus status) => status switch
        {
            PrinterStatus.PrinterConfigurationLoaded => ("Printer configuration loaded", "The printer configuration was successfully loaded from the application settings."),
            PrinterStatus.PrinterNotConfigured => ("Not Configured", "The printer is not configured."),
            PrinterStatus.PrintStarted => ("Printing", ""),
            PrinterStatus.PrinterOffline => ("Offline", "The printer is currently offline."),
            PrinterStatus.PrinterPaused => ("Paused", "The printer is paused."),
            PrinterStatus.PrinterResuming => ("Resuming", "The printer is resuming from a paused state."),
            PrinterStatus.PrinterDataSent => ("Printer data sent", "The system has transmitted data to the printer and is waiting for a response."),
            PrinterStatus.PrinterDataReceived => ("Printer response received", "The system received data from the printer."),
            PrinterStatus.PrinterConnecting => ("Connecting to printer", "The system is attempting to establish a connection with the printer."),
            PrinterStatus.PrinterConnected => ("Connected", "The printer is connected."),
            PrinterStatus.PrinterNotDetected => ("Printer not detected", "No printer device was found on the specified communication port."),
            PrinterStatus.PrinterDisconnected => ("Disconnected", "The printer is disconnected."),
            PrinterStatus.PrintCompleted => ("Print Completed", "The print job has completed successfully."),
            PrinterStatus.PrintFailed => ("Print Failed", "The print job has failed."),
            PrinterStatus.PrinterError => ("Error", "An error occurred with the printer."),
            _ => ("Unknown Status", "The printer status is unknown.")
        };

    }
}
