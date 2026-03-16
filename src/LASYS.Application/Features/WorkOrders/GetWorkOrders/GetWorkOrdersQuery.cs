using LASYS.Application.Common.Results;
using MediatR;

namespace LASYS.Application.Features.WorkOrders.GetWorkOrders
{
    public sealed record GetWorkOrdersQuery : IRequest<Result<List<WorkOrderItemResponse>>>;
}
