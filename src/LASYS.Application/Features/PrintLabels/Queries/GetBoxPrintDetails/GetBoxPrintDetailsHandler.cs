using Dapper;
using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Results;
using LASYS.Application.Interfaces.Persistence;
using MediatR;

namespace LASYS.Application.Features.PrintLabels.Queries.GetBoxPrintDetails
{
    public sealed class GetBoxPrintDetailsHandler : IRequestHandler<GetBoxPrintDetailsQuery, Result<PrintDetailsDto?>>
    {
        private readonly IDbConnectionFactory _factory;
        public GetBoxPrintDetailsHandler(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<Result<PrintDetailsDto?>> Handle(GetBoxPrintDetailsQuery request, CancellationToken cancellationToken)
        {
            string tableName = GetTableName(request.BoxType);
            string sql = $@"
            SELECT 
                ITEM_CODE AS ItemCode,
                LOT_NO AS LotNo,
                COUNT(CASE WHEN LABEL_STATUS IN ('Original','Replacement','Additional','Returned') THEN 1 END) AS TotalPassed,
                COUNT(CASE WHEN LABEL_STATUS IN ('Failed During Printing','Failed After Printing') THEN 1 END) AS TotalFailed,
                COUNT(CASE WHEN LABEL_STATUS IN ('First', 'Last') THEN 1 END) AS TotalSample,
                COUNT(CASE WHEN LABEL_STATUS IN 
                    ('First','Last','Original','Replacement','Additional','Failed During Printing','Failed After Printing','Returned') THEN 1 END) AS TotalPrinted,
                IFNULL(MAX(SEQUENCE_NUMBER) + 1, 1) AS NextSequence,
                IFNULL(MAX(CASE WHEN LABEL_STATUS IN ('Failed During Printing','Failed After Printing','First','Last') THEN BATCH_NUMBER END), 1) AS BatchNumber,
                IFNULL(MAX(SET_NUMBER) + 1, 1) AS SetNumber
            FROM {tableName} 
            WHERE ITEM_CODE = @ItemCode AND LOT_NO = @LotNo;";
            try
            {
                using var connection = await _factory.CreateConnectionAsync();
                var result = await connection.QueryFirstOrDefaultAsync<PrintDetailsDto?>(sql, new { request.ItemCode, request.LotNo });
                if (result is null)
                {
                    return Result.Failure<PrintDetailsDto?>("No data found for the specified item code and lot number.");
                }
                return Result.Success<PrintDetailsDto?>(result);
            }
            catch (Exception ex)
            {
                return Result.Failure<PrintDetailsDto?>($"An error occurred while retrieving print details.\n{ex.Message}");
            }

        }

        private static string GetTableName(BoxType boxType) => boxType switch
        {
            BoxType.UnitBox => "prdprnt_ub_labels_tcl",
            BoxType.AdditionalUnitBox => "prdprnt_aub_labels_tcl",
            BoxType.OuterUnitBox => "prdprnt_oub_labels_tcl",
            BoxType.CartonBox => "prdprnt_cb_labels_tcl",
            BoxType.AdditionalCartonBox => "prdprnt_acb_labels_tcl",
            BoxType.OuterCartonBox => "prdprnt_ocb_labels_tcl",
            _ => throw new ArgumentOutOfRangeException(nameof(boxType))
        };
    }
}
