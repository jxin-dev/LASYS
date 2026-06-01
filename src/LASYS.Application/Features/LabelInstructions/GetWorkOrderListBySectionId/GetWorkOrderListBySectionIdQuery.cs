using LASYS.Application.Common.Pagination;
using LASYS.Application.Common.Results;
using LASYS.Application.Features.LabelInstructions.GetWorkOrderListBySectionId;
using MediatR;

namespace LASYS.Application.Features.LabelInstructions.GetLabelInstructionsBySectionId
{
    public sealed record GetWorkOrderListBySectionIdQuery: IRequest<Result<PaginatedList<WorkOrderItem>>>;
}
