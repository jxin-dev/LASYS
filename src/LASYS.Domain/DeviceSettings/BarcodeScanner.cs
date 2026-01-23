namespace LASYS.Domain.DeviceSettings
{
    public class BarcodeScanner
    {
        public bool Enabled { get; set; } = true;
        public string Port { get; set; } = "COM3";
        public int BaudRate { get; set; } = 9600;
        public bool AutoScan { get; set; } = false;
    }
}
