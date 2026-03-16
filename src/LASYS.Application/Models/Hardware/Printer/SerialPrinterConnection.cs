using System.Text.Json.Serialization;

namespace LASYS.Application.Models.Hardware.Printer
{
    public class SerialPrinterConnection : PrinterConnection
    {
        [JsonIgnore]
        public override string InterfaceType => "Serial COM";
        public string ComPort { get; set; } = "COM50";
        public int BaudRate { get; set; } = 9600;
        public string Parameters { get; set; } = "8N1";

    }
}
