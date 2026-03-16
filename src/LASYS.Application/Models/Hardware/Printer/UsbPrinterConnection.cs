using System.Text.Json.Serialization;

namespace LASYS.Application.Models.Hardware.Printer
{
    public class UsbPrinterConnection : PrinterConnection
    {
        [JsonIgnore]
        public override string InterfaceType => "Usb";
        public string UsbId { get; set; } = string.Empty;
    }
}
