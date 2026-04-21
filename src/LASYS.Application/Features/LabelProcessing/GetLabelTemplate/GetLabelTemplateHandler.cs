using Dapper;
using LASYS.Application.Common.Results;
using LASYS.Application.Features.LabelProcessing.GetLabelTemplate;
using LASYS.Application.Interfaces.Persistence;
using MediatR;

namespace LASYS.Application.Features.LabelProcessing.LoadLabelTemplate
{
    public class GetLabelTemplateHandler : IRequestHandler<GetLabelTemplateQuery, Result<string>>
    {
        private readonly IDbConnectionFactory _factory;
        public GetLabelTemplateHandler(IDbConnectionFactory factory)
        {
            _factory = factory;
        }
        public async Task<Result<string>> Handle(GetLabelTemplateQuery request, CancellationToken cancellationToken)
        {
            //var connection = await _factory.CreateConnectionAsync();
            //var sql = "";
            //var templatePath = await connection.QuerySingleOrDefaultAsync<string>(sql);

            //if (templatePath is null)
            //    return Result.Failure<string>($"No template path found for WorkOrderId: {request.WorkOrderId}");
           
            //var templatePath = ‪@"C:\Users\ITC - JAYSON OLICIA\Desktop\SRFF2032_ub_w manufacturedate.lbl";
            var templatePath = @"D:\ITC\LASYS\prn_template.prn";
           
            return Result.Success(templatePath);
        }

    }
}
