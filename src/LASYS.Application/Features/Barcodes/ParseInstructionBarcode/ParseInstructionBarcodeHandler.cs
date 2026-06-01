using LASYS.Application.Common.Results;
using LASYS.Application.Features.Barcodes.Common.Interfaces;
using LASYS.Application.Features.Barcodes.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LASYS.Application.Features.Barcodes.ParseInstructionBarcode
{
    public sealed class ParseInstructionBarcodeHandler : IRequestHandler<ParseInstructionBarcodeQuery, Result<InstructionBarcodeResult>>
    {
        private readonly ILogger<ParseInstructionBarcodeHandler> _logger;
        private readonly IInstructionBarcodeParser _parser;

        public ParseInstructionBarcodeHandler(ILogger<ParseInstructionBarcodeHandler> logger, IInstructionBarcodeParser parser)
        {
            _logger = logger;
            _parser = parser;
        }

        public Task<Result<InstructionBarcodeResult>> Handle(ParseInstructionBarcodeQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result = _parser.Parse(request.Barcode);
                return Task.FromResult(Result.Success(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing instruction barcode.");
                return Task.FromResult(Result.Failure<InstructionBarcodeResult>(ex.Message));
            }

        }
    }
}
