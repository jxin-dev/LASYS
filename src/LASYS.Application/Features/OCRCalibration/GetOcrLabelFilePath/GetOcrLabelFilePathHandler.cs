using Dapper;
using LASYS.Application.Common.Results;
using LASYS.Application.Interfaces.Persistence;
using MediatR;

namespace LASYS.Application.Features.OCRCalibration.GetOcrLabelFilePath
{
    public class GetOcrLabelFilePathHandler : IRequestHandler<GetOcrLabelFilePathQuery, Result<string?>>
    {
        private readonly IDbConnectionFactory _factory;

        public GetOcrLabelFilePathHandler(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<Result<string?>> Handle(GetOcrLabelFilePathQuery request, CancellationToken cancellationToken)
        {
            var sql = @"
            SELECT FilePath
            FROM (
                SELECT ITEM_CODE, MASTERLABEL_REVISION_NUMBER, 'CASE' AS BoxType, CASE_LABEL_FILE_PATH AS FilePath
                FROM pre_masterlabels_tcl

                UNION ALL

                SELECT ITEM_CODE, MASTERLABEL_REVISION_NUMBER, 'UB', UB_LABEL_FILE_PATH
                FROM pre_masterlabels_tcl

                UNION ALL

                SELECT ITEM_CODE, MASTERLABEL_REVISION_NUMBER, 'OUB', OUB_LABEL_FILE_PATH
                FROM pre_masterlabels_tcl

                UNION ALL

                SELECT ITEM_CODE, MASTERLABEL_REVISION_NUMBER, 'AUB', AUB_LABEL_FILE_PATH
                FROM pre_masterlabels_tcl

                UNION ALL

                SELECT ITEM_CODE, MASTERLABEL_REVISION_NUMBER, 'CB', CB_LABEL_FILE_PATH
                FROM pre_masterlabels_tcl

                UNION ALL

                SELECT ITEM_CODE, MASTERLABEL_REVISION_NUMBER, 'OCB', OCB_LABEL_FILE_PATH
                FROM pre_masterlabels_tcl

                UNION ALL

                SELECT ITEM_CODE, MASTERLABEL_REVISION_NUMBER, 'ACB', ACB_LABEL_FILE_PATH
                FROM pre_masterlabels_tcl
            ) t
            WHERE t.ITEM_CODE = @ItemCode
              AND t.MASTERLABEL_REVISION_NUMBER = @RevisionNumber
              AND t.BoxType = @BoxType
            LIMIT 1;
        ";
            using var connection = await _factory.CreateConnectionAsync();

            var filePath = await connection.QueryFirstOrDefaultAsync<string>(
                sql,
                new
                {
                    request.ItemCode,
                    request.RevisionNumber,
                    request.BoxType
                });

            if (string.IsNullOrWhiteSpace(filePath))
            {
                return Result.Failure<string?>("No file path is available for the selected OCR item.");
            }

            return Result.Success<string?>(filePath);

        }
    }
}
