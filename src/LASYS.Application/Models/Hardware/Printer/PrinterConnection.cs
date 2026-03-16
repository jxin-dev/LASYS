using System.Text.Json.Serialization;

namespace LASYS.Application.Models.Hardware.Printer
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "Connection")]
    [JsonDerivedType(typeof(SerialPrinterConnection), "Serial COM")]
    [JsonDerivedType(typeof(UsbPrinterConnection), "Usb")]
    public abstract class PrinterConnection
    {
        public abstract string InterfaceType { get; }
    }
}
