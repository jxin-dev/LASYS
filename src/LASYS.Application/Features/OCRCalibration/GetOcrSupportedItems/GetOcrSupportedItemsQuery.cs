using LASYS.Application.Common.Pagination;
using LASYS.Application.Common.Results;
using MediatR;

namespace LASYS.Application.Features.OCRCalibration.GetOcrSupportedItems
{
    public record GetOcrSupportedItemsQuery : IRequest<Result<PaginatedList<OcrSupportedItemDto>>>;
}
