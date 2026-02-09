using static SATOPrinterAPI.Printer;

namespace LASYS.Infrastructure.Printing;

public class PrinterSettings
{
    public string PrinterName { get; set; } = "SATO CL612e 305dpi";
    public InterfaceType Interface { get; set; } = InterfaceType.USB;
    public string? PortIDOrIP { get; set; }
}
