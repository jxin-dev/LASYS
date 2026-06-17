using MediatR;

namespace LASYS.Application.Features.BarcodeValidation.ValidateInstructionBarcode
{
    public sealed record ValidateInstructionBarcodeQuery(string BarcodeScannedText) : IRequest<BarcodeValidationResult>;
}
