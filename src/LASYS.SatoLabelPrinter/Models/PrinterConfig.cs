using LASYS.SatoLabelPrinter.Interfaces;

namespace LASYS.SatoLabelPrinter.Models
{
    public class PrinterConfig
    {
        public IPrinterConnection? SatoPrinter { get; set; }
    }
}
