using LASYS.Application.Common.Results;
using LASYS.Application.Interfaces.Persistence;
using MediatR;
using Dapper;

namespace LASYS.Application.Features.WorkOrders.GetWorkOrders
{
    public sealed class GetWorkOrdersHandler : IRequestHandler<GetWorkOrdersQuery, Result<List<WorkOrderItemResponse>>>
    {
        private readonly IDbConnectionFactory _factory;
        public GetWorkOrdersHandler(IDbConnectionFactory factory)
        {
            _factory = factory;
        }
        public async Task<Result<List<WorkOrderItemResponse>>> Handle(GetWorkOrdersQuery request, CancellationToken cancellationToken)
        {
            using var connection = await _factory.CreateConnectionAsync();

            const string sql = "select * from ";

            var workorders = await connection.QueryAsync<WorkOrderItemResponse>(sql);
            return Result.Success(workorders.ToList());
        }
    }
}
