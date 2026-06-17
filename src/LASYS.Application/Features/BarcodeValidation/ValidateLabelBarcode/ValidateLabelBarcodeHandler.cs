using MediatR;

namespace LASYS.Application.Features.BarcodeValidation.ValidateLabelBarcode
{
    public sealed class ValidateLabelBarcodeHandler : IRequestHandler<ValidateLabelBarcodeQuery, BarcodeValidationResult>
    {
        private readonly Gs1BarcodeParser _barcodeParser;
        public ValidateLabelBarcodeHandler(Gs1BarcodeParser barcodeParser)
        {
            _barcodeParser = barcodeParser;
        }
        public Task<BarcodeValidationResult> Handle(ValidateLabelBarcodeQuery request, CancellationToken cancellationToken)
        {
            var result = _barcodeParser.Parse(request.BarcodeScannedText, BarcodeType.Label);

            return Task.FromResult(result);
        }
    }
}
