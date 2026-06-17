using MediatR;

namespace LASYS.Application.Features.BarcodeValidation.ValidateInstructionBarcode
{
    public sealed class ValidateInstructionBarcodeHandler : IRequestHandler<ValidateInstructionBarcodeQuery, BarcodeValidationResult>
    {
        private readonly Gs1BarcodeParser _barcodeParser;
        public ValidateInstructionBarcodeHandler(Gs1BarcodeParser barcodeParser)
        {
            _barcodeParser = barcodeParser;
        }
        public Task<BarcodeValidationResult> Handle(ValidateInstructionBarcodeQuery request, CancellationToken cancellationToken)
        {
            var result = _barcodeParser.Parse(request.BarcodeScannedText, BarcodeType.Instruction);
            return Task.FromResult(result);
        }
    }
}
