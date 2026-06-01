using LASYS.Application.Common.Results;
using LASYS.Application.Features.Barcodes.Common.Models;
using MediatR;

namespace LASYS.Application.Features.Barcodes.ParseInstructionBarcode
{
    public sealed record ParseInstructionBarcodeQuery(string Barcode) : IRequest<Result<InstructionBarcodeResult>>;
}
