using LASYS.Application.Features.Barcodes.Common.Models;

namespace LASYS.Application.Features.Barcodes.Common.Interfaces
{
    public interface IInstructionBarcodeParser
    {
        InstructionBarcodeResult Parse(string barcode);
    }
}
