using LASYS.Application.Interfaces;

namespace LASYS.Application.Contracts
{
    public class PrinterConfig
    {
        public IPrinterConnection? SatoPrinter { get; set; }
    }
}
