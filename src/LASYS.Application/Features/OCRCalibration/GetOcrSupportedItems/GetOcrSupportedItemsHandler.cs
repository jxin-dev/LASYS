using Dapper;
using LASYS.Application.Common.Results;
using LASYS.Application.Interfaces.Persistence;
using MediatR;

namespace LASYS.Application.Features.OCRCalibration.GetOcrSupportedItems
{
    public class GetOcrSupportedItemsHandler : IRequestHandler<GetOcrSupportedItemsQuery, Result<List<OcrSupportedItemDto>>>
    {
        private readonly IDbConnectionFactory _factory;
        public GetOcrSupportedItemsHandler(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<Result<List<OcrSupportedItemDto>>> Handle(GetOcrSupportedItemsQuery request, CancellationToken cancellationToken)
        {
            var sql = @"
            SELECT ItemCode, RevisionNumber, BoxType, FilePath FROM
                (SELECT ITEM_CODE AS ItemCode, MASTERLABEL_REVISION_NUMBER AS RevisionNumber, 'CASE' AS BoxType, CASE_LABEL_FILE_PATH AS FilePath, IS_CASE_OCR_SUPPORTED AS IsSupported FROM pre_masterlabels_tcl
                UNION ALL SELECT ITEM_CODE, MASTERLABEL_REVISION_NUMBER,'UB', UB_LABEL_FILE_PATH, IS_UB_OCR_SUPPORTED FROM pre_masterlabels_tcl
                UNION ALL SELECT ITEM_CODE, MASTERLABEL_REVISION_NUMBER, 'OUB', OUB_LABEL_FILE_PATH, IS_OUB_OCR_SUPPORTED FROM pre_masterlabels_tcl
                UNION ALL SELECT ITEM_CODE, MASTERLABEL_REVISION_NUMBER, 'AUB', AUB_LABEL_FILE_PATH, IS_AUB_OCR_SUPPORTED FROM pre_masterlabels_tcl
                UNION ALL SELECT ITEM_CODE, MASTERLABEL_REVISION_NUMBER, 'CB', CB_LABEL_FILE_PATH, IS_CB_OCR_SUPPORTED FROM pre_masterlabels_tcl
                UNION ALL SELECT ITEM_CODE, MASTERLABEL_REVISION_NUMBER, 'OCB', OCB_LABEL_FILE_PATH, IS_OCB_OCR_SUPPORTED FROM pre_masterlabels_tcl
                UNION ALL SELECT ITEM_CODE, MASTERLABEL_REVISION_NUMBER, 'ACB', ACB_LABEL_FILE_PATH, IS_ACB_OCR_SUPPORTED FROM pre_masterlabels_tcl) OCRItems 
            WHERE IsSupported = 0 AND FilePath IS NOT NULL AND TRIM(FilePath) <> '' ORDER BY ItemCode, BoxType, RevisionNumber DESC;";
            using var connection = await _factory.CreateConnectionAsync();
            var items = (await connection.QueryAsync<OcrSupportedItemDto>(sql)).ToList();
            return Result.Success(items);
        }
    }
}
