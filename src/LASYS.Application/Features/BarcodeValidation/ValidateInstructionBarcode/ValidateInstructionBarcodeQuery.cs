using MediatR;

namespace LASYS.Application.Features.BarcodeValidation.ValidateInstructionBarcode
{
    public sealed record ValidateInstructionBarcodeQuery(string BarcodeScannedText, bool IsEumdr) : IRequest<BarcodeValidationResult>;
}
