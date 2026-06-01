using Dapper;
using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Models;
using LASYS.Application.Common.Results;
using LASYS.Application.Interfaces.Persistence;
using LASYS.Application.Interfaces.Persistence.Repositories;
using LASYS.Application.Interfaces.Persistence.TableMappings;
using MediatR;

namespace LASYS.Infrastructure.Persistence.Repositories
{
    public sealed class PrintLabelRepository : IPrintLabelRepository
    {
        private readonly IDbConnectionFactory _factory;
        private readonly IPrintTableResolver _printTableNameResolver;

        public PrintLabelRepository(IDbConnectionFactory factory, IPrintTableResolver printTableNameResolver)
        {
            _factory = factory;
            _printTableNameResolver = printTableNameResolver;
        }

        public async Task<PrintDetails> GetDetailsAsync(string itemCode, string lotNo, BoxType boxType)
        {
            string tableName = _printTableNameResolver.GetTableName(boxType);
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
                var result = await connection.QueryFirstOrDefaultAsync<PrintDetails>(sql, new { itemCode, lotNo });
                return result ?? throw new Exception("Print details not found.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving print details: {ex.Message}", ex);
            }
        }
    }
}
