using LASYS.Application.Common.Results;
using MediatR;

namespace LASYS.Application.Features.LabelProcessing.BarcodeScan
{
    public record BarcodeScanQuery(string BarcodeData) : IRequest<Result<BarcodeScanResult>>;
}
