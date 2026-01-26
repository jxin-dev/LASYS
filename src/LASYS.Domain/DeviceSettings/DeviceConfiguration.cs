namespace LASYS.Domain.DeviceSettings
{
    public class DeviceConfiguration
    {
        public BarcodeScannerConfig BarcodeScanner { get; set; } = new();
        public CameraConfig Camera { get; set; } = new();
    }
}
