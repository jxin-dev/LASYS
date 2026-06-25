using Dapper;
using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Pagination;
using LASYS.Application.Common.Results;
using LASYS.Application.Interfaces.Context;
using LASYS.Application.Interfaces.Persistence;
using MediatR;

namespace LASYS.Application.Features.OCRCalibration.GetOcrSupportedItems
{
    public class GetOcrSupportedItemsHandler : IRequestHandler<GetOcrSupportedItemsQuery, Result<PaginatedList<OcrSupportedItemDto>>>
    {
        private readonly IDbConnectionFactory _factory;
        private readonly ICurrentUser _currentUser;
        public GetOcrSupportedItemsHandler(IDbConnectionFactory factory, ICurrentUser currentUser)
        {
            _factory = factory;
            _currentUser = currentUser;
        }

        public async Task<Result<PaginatedList<OcrSupportedItemDto>>> Handle(GetOcrSupportedItemsQuery request, CancellationToken cancellationToken)
        {
            var sectionId = _currentUser.SectionId;

            var sql = @"
            SELECT 
                NULLIF(
                    CONCAT_WS(',',
                        CASE WHEN TRIM(IFNULL(pr.PRINT_CASE_LABEL_FLAG, '')) <> '' AND p.IS_CASE_OCR_SUPPORTED = 1 THEN 'CASE' END,
                        CASE WHEN TRIM(IFNULL(t1.UB_LBL_INS_CODE, ''))  <> '' AND p.IS_UB_OCR_SUPPORTED = 1 THEN 'UB' END,
                        CASE WHEN TRIM(IFNULL(t1.AUB_LBL_INS_CODE, '')) <> '' AND p.IS_AUB_OCR_SUPPORTED = 1 THEN 'AUB' END,
                        CASE WHEN TRIM(IFNULL(t1.OUB_LBL_INS_CODE, '')) <> '' AND p.IS_OUB_OCR_SUPPORTED = 1 THEN 'OUB' END,
                        CASE WHEN TRIM(IFNULL(t1.CB_LBL_INS_CODE, ''))  <> '' AND p.IS_CB_OCR_SUPPORTED = 1 THEN 'CB' END,
                        CASE WHEN TRIM(IFNULL(t1.ACB_LBL_INS_CODE, '')) <> '' AND p.IS_ACB_OCR_SUPPORTED = 1 THEN 'ACB' END,
                        CASE WHEN TRIM(IFNULL(t1.OCB_LBL_INS_CODE, '')) <> '' AND p.IS_OCB_OCR_SUPPORTED = 1 THEN 'OCB' END
                    ),
                    NULL
                ) AS AvailableBoxTypes,
	            t1.ITEM_CODE AS ItemCode,
                t1.LABEL_INS_REV_NUMBER AS LabelInstructionRevNumber,
                p.MASTERLABEL_REVISION_NUMBER AS MasterLabelRevNumber
            FROM ppt_lbl_instructn_plns_hst t1 
            INNER JOIN pre_masterlabels_tcl p
                ON p.ITEM_CODE = t1.ITEM_CODE
                AND p.MASTERLABEL_REVISION_NUMBER = t1.MASTER_LABEL_REVISION_NUMBER
            INNER JOIN pre_tpc_products_tcl pr
                ON pr.ITEM_CODE = t1.ITEM_CODE
                AND pr.MASTERLABEL_REVISION_NUMBER = t1.MASTER_LABEL_REVISION_NUMBER
            
            WHERE NOT EXISTS (
                SELECT 1
                FROM ppt_lbl_instructn_plns_hst t2
                WHERE t2.ITEM_CODE = t1.ITEM_CODE
                  AND t2.MASTER_LABEL_REVISION_NUMBER > t1.MASTER_LABEL_REVISION_NUMBER
            )
            AND ((t1.UB_LBL_INS_STATUS IN ('For Printing','Printed') AND t1.UB_LBL_INS_VERDICT = 'Approved' AND t1.UB_Lbl_Status IN ('Not Printed','Partially Printed','Completely Printed')) OR
	            (t1.OUB_LBL_INS_STATUS IN ('For Printing','Printed') AND t1.OUB_LBL_INS_VERDICT = 'Approved' AND t1.OUB_Lbl_Status IN ('Not Printed','Partially Printed','Completely Printed')) OR
	            (t1.OCB_LBL_INS_STATUS IN ('For Printing','Printed') AND t1.OCB_LBL_INS_VERDICT = 'Approved' AND t1.OCB_Lbl_Status IN ('Not Printed','Partially Printed','Completely Printed')) OR
	            (t1.CB_LBL_INS_STATUS IN ('For Printing','Printed') AND t1.CB_LBL_INS_VERDICT = 'Approved' AND t1.CB_Lbl_Status IN ('Not Printed','Partially Printed','Completely Printed')))
            AND pr.ACTIVE_FLAG IS NOT NULL
            AND FIND_IN_SET(@SectionId, pr.SECTION_ASSIGNMENTS) > 0
            AND (
                (TRIM(IFNULL(t1.UB_LBL_INS_CODE, '')) <> '' AND p.IS_UB_OCR_SUPPORTED = 1)
             OR (TRIM(IFNULL(t1.AUB_LBL_INS_CODE, '')) <> '' AND p.IS_AUB_OCR_SUPPORTED = 1)
             OR (TRIM(IFNULL(t1.OUB_LBL_INS_CODE, '')) <> '' AND p.IS_OUB_OCR_SUPPORTED = 1)
             OR (TRIM(IFNULL(t1.CB_LBL_INS_CODE, '')) <> '' AND p.IS_CB_OCR_SUPPORTED = 1)
             OR (TRIM(IFNULL(t1.ACB_LBL_INS_CODE, '')) <> '' AND p.IS_ACB_OCR_SUPPORTED = 1)
             OR (TRIM(IFNULL(t1.OCB_LBL_INS_CODE, '')) <> '' AND p.IS_OCB_OCR_SUPPORTED = 1)
             OR (TRIM(IFNULL(pr.PRINT_CASE_LABEL_FLAG, '')) <> '' AND p.IS_CASE_OCR_SUPPORTED = 1) -- if CASE is supported
            )
            GROUP BY t1.ITEM_CODE, t1.LABEL_INS_REV_NUMBER, t1.MASTER_LABEL_REVISION_NUMBER 
            ORDER BY t1.history_datetime DESC";

            using var connection = await _factory.CreateConnectionAsync();
            var rows = await connection.QueryAsync(sql, new { SectionId = sectionId });
            var result = rows.Select(x => new OcrSupportedItemDto
            {
                ItemCode = x.ItemCode,
                LabelInstructionRevNumber = x.LabelInstructionRevNumber,
                MasterLabelRevNumber = x.MasterLabelRevNumber,
                AvailableBoxTypes = string.IsNullOrWhiteSpace((string?)x.AvailableBoxTypes)
                ? []
                : ((string)x.AvailableBoxTypes)
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Where(s => BoxTypeMap.ContainsKey(s.Trim()))
                    .Select(s => BoxTypeMap[s.Trim()])
                    .ToList()
            }).ToList();

            if (result is null || !result.Any())
            {
                return Result.Failure<PaginatedList<OcrSupportedItemDto>>("No items found for the specified section.");
            }
            var items = new PaginatedList<OcrSupportedItemDto>(result, result.Count, 1, 10);
            return Result.Success(items);
        }

        private static readonly Dictionary<string, BoxType> BoxTypeMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["CASE"] = BoxType.CaseLabel,
            ["UB"] = BoxType.UnitBox,
            ["AUB"] = BoxType.AdditionalUnitBox,
            ["OUB"] = BoxType.OuterUnitBox,
            ["CB"] = BoxType.CartonBox,
            ["ACB"] = BoxType.AdditionalCartonBox,
            ["OCB"] = BoxType.OuterCartonBox,
        };
    }
}
