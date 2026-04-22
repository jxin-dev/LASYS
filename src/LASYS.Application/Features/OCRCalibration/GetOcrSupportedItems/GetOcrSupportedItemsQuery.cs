using LASYS.Application.Common.Results;
using MediatR;

namespace LASYS.Application.Features.OCRCalibration.GetOcrSupportedItems
{
    public record GetOcrSupportedItemsQuery : IRequest<Result<List<OcrSupportedItemDto>>>
    {
    }
}
