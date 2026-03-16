using LASYS.Application.Common.Results;
using MediatR;

namespace LASYS.Application.Features.WorkOrders.GetWorkOrderDetails
{
    public record GetWorkOrderDetailsQuery(int WorkOrderId) : IRequest<Result<WorkOrderDetailsResponse>>;
}
