using Dapper;
using LASYS.Application.Common.Results;
using LASYS.Application.Interfaces.Persistence;
using MediatR;

namespace LASYS.Application.Features.WorkOrders.GetWorkOrderDetails
{
    public class GetWorkOrderDetailsHandler : IRequestHandler<GetWorkOrderDetailsQuery, Result<WorkOrderDetailsResponse>>
    {
        private readonly IDbConnectionFactory _factory;
        public GetWorkOrderDetailsHandler(IDbConnectionFactory factory)
        {
            _factory = factory;
        }
        public async Task<Result<WorkOrderDetailsResponse>> Handle(GetWorkOrderDetailsQuery request, CancellationToken cancellationToken)
        {
            using var connection = await _factory.CreateConnectionAsync();

            var workOrderId = request.WorkOrderId;
            const string sql = "select * from where id = ";

            var workorder = await connection.QuerySingleOrDefaultAsync<WorkOrderDetailsResponse>(sql);
            if (workorder == null)
            {
                return Result.Failure<WorkOrderDetailsResponse>("Work order not found.");
            }
            return Result.Success(workorder);
        }
    }
}
