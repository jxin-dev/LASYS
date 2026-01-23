namespace LASYS.Domain.DeviceSettings
{
    public class DeviceConfiguration
    {
        public BarcodeScanner BarcodeScanner { get; set; } = new();
        public Camera Camera { get; set; } = new();
    }
}
