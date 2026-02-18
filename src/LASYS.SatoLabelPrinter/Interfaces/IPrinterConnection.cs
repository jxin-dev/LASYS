using System.Text.Json.Serialization;

namespace LASYS.SatoLabelPrinter.Interfaces
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "InterfaceType")]
    [JsonDerivedType(typeof(SerialPrinterConnection), "Serial COM")]
    [JsonDerivedType(typeof(UsbPrinterConnection), "Usb")]
    public interface IPrinterConnection
    {
        //string InterfaceType { get; }
    }
    public class SerialPrinterConnection : IPrinterConnection
    {
        //public string InterfaceType => "Serial COM";
        public string ComPort { get; set; } = "COM50";
        public int BaudRate { get; set; } = 9600;
        public string Parameters { get; set; } = "8N1";

    }
    public class UsbPrinterConnection : IPrinterConnection
    {
        //public string InterfaceType => "Usb";
        public string UsbId { get; set; } = string.Empty;
    }
}
