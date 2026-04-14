using LASYS.Application.Common.Results;
using MediatR;

namespace LASYS.Application.Features.LabelProcessing.GetWorkOrders
{
    public record GetWorkOrdersQuery(string filter, int pageSize = 10, int pageNo = 1) : IRequest<Result<IEnumerable<GetWorkOrdersResult>>;
}
