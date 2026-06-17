using MediatR;

namespace LASYS.Application.Features.BarcodeValidation.ValidateLabelBarcode
{
    public sealed record ValidateLabelBarcodeQuery(string? BarcodeScannedText, bool IsEumdr) : IRequest<BarcodeValidationResult>;
}
