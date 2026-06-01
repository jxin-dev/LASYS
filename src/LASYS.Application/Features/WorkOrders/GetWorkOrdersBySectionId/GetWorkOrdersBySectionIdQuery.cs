using LASYS.Application.Common.Pagination;
using LASYS.Application.Common.Results;
using MediatR;

namespace LASYS.Application.Features.WorkOrders.GetWorkOrdersBySectionId
{
    public sealed record GetWorkOrdersBySectionIdQuery() : IRequest<Result<PaginatedList<Product>>>;
}
