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
            var sql = @"SELECT ITEM_CODE AS ItemCode, MASTERLABEL_REVISION_NUMBER AS RevisionNumber, 'CASE' AS BoxType, COALESCE(CASE_LABEL_FILE_PATH, 'NO FILE PATH') AS FilePath FROM pre_masterlabels_tcl WHERE IS_CASE_OCR_SUPPORTED = 1 UNION ALL 
                        SELECT ITEM_CODE, MASTERLABEL_REVISION_NUMBER, 'UB', COALESCE(UB_LABEL_FILE_PATH, 'NO FILE PATH') FROM pre_masterlabels_tcl WHERE IS_UB_OCR_SUPPORTED = 1 UNION ALL 
                        SELECT ITEM_CODE, MASTERLABEL_REVISION_NUMBER, 'OUB', COALESCE(OUB_LABEL_FILE_PATH, 'NO FILE PATH') FROM pre_masterlabels_tcl WHERE IS_OUB_OCR_SUPPORTED = 1 UNION ALL 
                        SELECT ITEM_CODE, MASTERLABEL_REVISION_NUMBER, 'AUB', COALESCE(AUB_LABEL_FILE_PATH, 'NO FILE PATH') FROM pre_masterlabels_tcl WHERE IS_AUB_OCR_SUPPORTED = 1 UNION ALL 
                        SELECT ITEM_CODE, MASTERLABEL_REVISION_NUMBER, 'CB', COALESCE(CB_LABEL_FILE_PATH, 'NO FILE PATH') FROM pre_masterlabels_tcl WHERE IS_CB_OCR_SUPPORTED = 1 UNION ALL 
                        SELECT ITEM_CODE, MASTERLABEL_REVISION_NUMBER, 'OCB', COALESCE(OCB_LABEL_FILE_PATH, 'NO FILE PATH') FROM pre_masterlabels_tcl WHERE IS_OCB_OCR_SUPPORTED = 1 UNION ALL 
                        SELECT ITEM_CODE, MASTERLABEL_REVISION_NUMBER, 'ACB', COALESCE(ACB_LABEL_FILE_PATH, 'NO FILE PATH') FROM pre_masterlabels_tcl WHERE IS_ACB_OCR_SUPPORTED = 1 
                        ORDER BY ItemCode , RevisionNumber DESC, BoxType;";
            using var connection = await _factory.CreateConnectionAsync();
            var items = (await connection.QueryAsync<OcrSupportedItemDto>(sql)).ToList();
            return Result.Success(items);
        }
    }
}
