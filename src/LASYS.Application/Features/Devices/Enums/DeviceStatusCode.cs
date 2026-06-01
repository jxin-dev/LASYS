namespace LASYS.Application.Features.Devices.Enums
{
    public enum DeviceStatusCode
    {
        Configuring,
        NotConfigured,
        NotDetected,
        Connected,
        Disconnected,
        Reconnecting,
        Timeout,
        Error,
        // Camera-specific
        CameraCapturing,
        CameraFocusing,
        // Barcode-specific
        Communicating,
        Scanning,
        Scanned,
        // Printer-specific
        ConfigurationLoaded,
        Started,
        Offline,
        Paused,
        Resuming,
        DataSent,
        DataReceived,
        Connecting,
        PrintCompleted,
        PrintFailed,
        PrinterError,
    }
}
