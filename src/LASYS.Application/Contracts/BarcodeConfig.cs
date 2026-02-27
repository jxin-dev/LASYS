namespace LASYS.Application.Contracts
{
    public class BarcodeConfig
    {
        public string Port { get; set; } = string.Empty;
        public int BaudRate { get; set; } = 9600;
    }
}
