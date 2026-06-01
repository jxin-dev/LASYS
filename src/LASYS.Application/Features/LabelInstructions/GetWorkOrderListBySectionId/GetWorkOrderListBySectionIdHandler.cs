using Dapper;
using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Pagination;
using LASYS.Application.Common.Results;
using LASYS.Application.Features.LabelInstructions.GetWorkOrderListBySectionId;
using LASYS.Application.Interfaces.Context;
using LASYS.Application.Interfaces.Persistence;
using MediatR;

namespace LASYS.Application.Features.LabelInstructions.GetLabelInstructionsBySectionId
{
    public sealed class GetWorkOrderListBySectionIdHandler : IRequestHandler<GetWorkOrderListBySectionIdQuery, Result<PaginatedList<WorkOrderItem>>>
    {
        private readonly IDbConnectionFactory _factory;

        private readonly ICurrentUser _currentUser;
        public GetWorkOrderListBySectionIdHandler(IDbConnectionFactory factory, ICurrentUser currentUser)
        {
            _factory = factory;
            _currentUser = currentUser;
        }
        public async Task<Result<PaginatedList<WorkOrderItem>>> Handle(GetWorkOrderListBySectionIdQuery request, CancellationToken cancellationToken)
        {
            var sectionId = _currentUser.SectionId;

            var sql = @"
            SELECT 
                NULLIF(
                    CONCAT_WS(',',
                        CASE WHEN TRIM(IFNULL(t1.UB_LBL_INS_CODE, ''))  <> '' THEN 'UB' END,
                        CASE WHEN TRIM(IFNULL(t1.AUB_LBL_INS_CODE, '')) <> '' THEN 'AUB' END,
                        CASE WHEN TRIM(IFNULL(t1.OUB_LBL_INS_CODE, '')) <> '' THEN 'OUB' END,
                        CASE WHEN TRIM(IFNULL(t1.CB_LBL_INS_CODE, ''))  <> '' THEN 'CB' END,
                        CASE WHEN TRIM(IFNULL(t1.ACB_LBL_INS_CODE, '')) <> '' THEN 'ACB' END,
                        CASE WHEN TRIM(IFNULL(t1.OCB_LBL_INS_CODE, '')) <> '' THEN 'OCB' END
                    ),
                    NULL
                ) AS AvailableBoxTypes,
                pr.SECTION_ASSIGNMENTS AS SectionAssignments,
                t1.LINE_CODE AS LineCode,
	            t1.ITEM_CODE AS ItemCode,
                t1.LOT_NO AS LotNo,
                t1.LABEL_INS_REV_NUMBER AS LabelInsRevNumber,
                p.MASTERLABEL_REVISION_NUMBER AS MasterLabelRevNumber,
                p.UDI AS Udi,
                IF(
                    pr.LABEL_TYPE_EUMDR_FLAG = 1 AND t1.MANUFACTURE_DATE IS NOT NULL,
                    t1.MANUFACTURE_DATE,
                    NULL
                ) AS ManufactureDate,
                t1.EXPIRY_DATE AS ExpirationDate,
                t1.PRODUCTION_DATE AS ProductionDate,
                t1.STERILIZATION_DATE AS SterilizationDate,
                t1.Target_Production_Quantity AS TargetProductionQuantity,
                                
                t1.UB_LBL_INS_CODE AS UbInstructionCode,
                t1.UB_LBL_INS_TRGT_PRNT_QTY AS UbTargetPrintQuantity,
                t1.UB_LBL_INS_PRNT_TYPE AS UbPrintType, 
                t1.UB_LBL_INS_VERDICT AS UbVerdict,
                t1.UB_LBL_INS_STATUS AS UbInstructionStatus,
                t1.UB_LBL_STATUS AS UbLabelStatus,
                t1.UB_LBL_INS_APPRVD_USR_CD AS UbApprovedBy,
                STR_TO_DATE(t1.UB_LBL_INS_APPRVD_DATETIME,  '%Y%m%d%H%i%s') AS UbDateApproved,

                t1.AUB_LBL_INS_CODE AS AubInstructionCode,
                t1.AUB_LBL_INS_TRGT_PRNT_QTY AS AubTargetPrintQuantity,
                t1.AUB_LBL_INS_PRNT_TYPE AS AubPrintType, 
                t1.AUB_LBL_INS_VERDICT AS AubVerdict,
                t1.AUB_LBL_INS_STATUS AS AubInstructionStatus,
                t1.AUB_LBL_STATUS AS AubLabelStatus,
                t1.AUB_LBL_INS_APPRVD_USR_CD AS AubApprovedBy,
                STR_TO_DATE(t1.AUB_LBL_INS_APPRVD_DATETIME,  '%Y%m%d%H%i%s') AS AubDateApproved,

                t1.OUB_LBL_INS_CODE AS OubInstructionCode,
                t1.OUB_LBL_INS_TRGT_PRNT_QTY AS OubTargetPrintQuantity,
                t1.OUB_LBL_INS_PRNT_TYPE AS OubPrintType, 
                t1.OUB_LBL_INS_VERDICT AS OubVerdict,
                t1.OUB_LBL_INS_STATUS AS OubInstructionStatus,
                t1.OUB_LBL_STATUS AS OubLabelStatus,
                t1.OUB_LBL_INS_APPRVD_USR_CD AS OubApprovedBy,
                STR_TO_DATE(t1.OUB_LBL_INS_APPRVD_DATETIME,  '%Y%m%d%H%i%s') AS OubDateApproved,

                t1.CB_LBL_INS_CODE AS CbInstructionCode,
                t1.CB_LBL_INS_TRGT_PRNT_QTY AS CbTargetPrintQuantity,
                t1.CB_LBL_INS_PRNT_TYPE AS CbPrintType, 
                t1.CB_LBL_INS_VERDICT AS CbVerdict,
                t1.CB_LBL_INS_STATUS AS CbInstructionStatus,
                t1.CB_LBL_STATUS AS CbLabelStatus,
                t1.CB_LBL_INS_APPRVD_USR_CD AS CbApprovedBy,
                STR_TO_DATE(t1.CB_LBL_INS_APPRVD_DATETIME,  '%Y%m%d%H%i%s') AS CbDateApproved,
                
                t1.ACB_LBL_INS_CODE AS AcbInstructionCode,
                t1.ACB_LBL_INS_TRGT_PRNT_QTY AS AcbTargetPrintQuantity,
                t1.ACB_LBL_INS_PRNT_TYPE AS AcbPrintType, 
                t1.ACB_LBL_INS_VERDICT AS AcbVerdict,
                t1.ACB_LBL_INS_STATUS AS AcbInstructionStatus,
                t1.ACB_LBL_STATUS AS AcbLabelStatus,
                t1.ACB_LBL_INS_APPRVD_USR_CD AS AcbApprovedBy,
                STR_TO_DATE(t1.ACB_LBL_INS_APPRVD_DATETIME,  '%Y%m%d%H%i%s') AS AcbDateApproved,

                t1.OCB_LBL_INS_CODE AS OcbInstructionCode,
                t1.OCB_LBL_INS_TRGT_PRNT_QTY AS OcbTargetPrintQuantity,
                t1.OCB_LBL_INS_PRNT_TYPE AS OcbPrintType, 
                t1.OCB_LBL_INS_VERDICT AS OcbVerdict,
                t1.OCB_LBL_INS_STATUS AS OcbInstructionStatus,
                t1.OCB_LBL_STATUS AS OcbLabelStatus,
                t1.OCB_LBL_INS_APPRVD_USR_CD AS OcbApprovedBy,
                STR_TO_DATE(t1.OCB_LBL_INS_APPRVD_DATETIME,  '%Y%m%d%H%i%s') AS OcbDateApproved

            FROM ppt_lbl_instructn_plns_hst t1

            LEFT JOIN ppt_lbl_instructn_plns_hst t2
                ON t1.ITEM_CODE = t2.ITEM_CODE
                AND t1.LOT_NO = t2.LOT_NO
                AND t1.MASTER_LABEL_REVISION_NUMBER < t2.MASTER_LABEL_REVISION_NUMBER
            INNER JOIN pre_masterlabels_tcl p
                ON p.ITEM_CODE = t1.ITEM_CODE
                AND p.MASTERLABEL_REVISION_NUMBER = t1.MASTER_LABEL_REVISION_NUMBER
            INNER JOIN pre_tpc_products_tcl pr
                ON pr.ITEM_CODE = t1.ITEM_CODE
                AND pr.MASTERLABEL_REVISION_NUMBER = t1.MASTER_LABEL_REVISION_NUMBER
            
            WHERE t2.ITEM_CODE IS NULL 
            AND ((t1.UB_LBL_INS_STATUS IN ('For Printing','Printed') AND t1.UB_LBL_INS_VERDICT = 'Approved' AND t1.UB_Lbl_Status IN ('Not Printed','Partially Printed','Completely Printed')) OR
	            (t1.OUB_LBL_INS_STATUS IN ('For Printing','Printed') AND t1.OUB_LBL_INS_VERDICT = 'Approved' AND t1.OUB_Lbl_Status IN ('Not Printed','Partially Printed','Completely Printed')) OR
	            (t1.OCB_LBL_INS_STATUS IN ('For Printing','Printed') AND t1.OCB_LBL_INS_VERDICT = 'Approved' AND t1.OCB_Lbl_Status IN ('Not Printed','Partially Printed','Completely Printed')) OR
	            (t1.CB_LBL_INS_STATUS IN ('For Printing','Printed') AND t1.CB_LBL_INS_VERDICT = 'Approved' AND t1.CB_Lbl_Status IN ('Not Printed','Partially Printed','Completely Printed')))
            AND pr.ACTIVE_FLAG IS NOT NULL
            AND FIND_IN_SET(@SectionId, pr.SECTION_ASSIGNMENTS) > 0
            GROUP BY t1.ITEM_CODE, t1.LOT_NO, t1.LABEL_INS_REV_NUMBER, t1.MASTER_LABEL_REVISION_NUMBER 
            ORDER BY t1.history_datetime DESC
            ";
            try
            {
                using var connection = await _factory.CreateConnectionAsync();
                var rows = await connection.QueryAsync(sql, new { SectionId = sectionId });
                var result = rows.Select(x => new WorkOrderItem
                {
                    //AvailableBoxTypes = x.AvailableBoxTypes,
                    SectionAssignments = x.SectionAssignments,
                    LineCode = x.LineCode,
                    ItemCode = x.ItemCode,
                    LotNo = x.LotNo,
                    LabelInsRevNumber = x.LabelInsRevNumber,
                    MasterLabelRevNumber = x.MasterLabelRevNumber,
                    Udi = x.Udi,
                    ManufactureDate = x.ManufactureDate,
                    ExpirationDate = x.ExpirationDate,
                    ProductionDate = x.ProductionDate,
                    SterilizationDate = x.SterilizationDate,
                    TargetProductionQuantity = x.TargetProductionQuantity,
                    Details = BuildDetails(x)
                }).ToList();

                if (result is null || !result.Any())
                {
                    return Result.Failure<PaginatedList<WorkOrderItem>>("No work orders found for the specified section.");
                }
                var items = new PaginatedList<WorkOrderItem>(result, result.Count, 1, 10);
                return Result.Success(items);

            }
            catch (Exception ex)
            {
                return Result.Failure<PaginatedList<WorkOrderItem>>("No work orders found for the specified section.");
            }
        }
        private static Dictionary<BoxType, BoxLabelInstructionDetails> BuildDetails(dynamic x)
        {
            var details = new Dictionary<BoxType, BoxLabelInstructionDetails>();

            if (x.UbInstructionCode != null && !string.IsNullOrEmpty(x.UbInstructionCode))
            {
                details[BoxType.UnitBox] = new BoxLabelInstructionDetails
                {
                    InstructionCode = x.UbInstructionCode,
                    TargetPrintQuantity = x.UbTargetPrintQuantity,
                    PrintType = x.UbPrintType,
                    Verdict = x.UbVerdict,
                    InstructionStatus = x.UbInstructionStatus,
                    LabelStatus = x.UbLabelStatus,
                    ApprovedBy = x.UbApprovedBy,
                    DateApproved = x.UbDateApproved
                };
            }

            if (x.AubInstructionCode != null && !string.IsNullOrEmpty(x.AubInstructionCode))
            {
                details[BoxType.AdditionalUnitBox] = new BoxLabelInstructionDetails
                {
                    InstructionCode = x.AubInstructionCode,
                    TargetPrintQuantity = x.AubTargetPrintQuantity,
                    PrintType = x.AubPrintType,
                    Verdict = x.AubVerdict,
                    InstructionStatus = x.AubInstructionStatus,
                    LabelStatus = x.AubLabelStatus,
                    ApprovedBy = x.AubApprovedBy,
                    DateApproved = x.AubDateApproved
                };
            }

            if (x.OubInstructionCode != null && !string.IsNullOrEmpty(x.OubInstructionCode))
            {
                details[BoxType.OuterUnitBox] = new BoxLabelInstructionDetails
                {
                    InstructionCode = x.OubInstructionCode,
                    TargetPrintQuantity = x.OubTargetPrintQuantity,
                    PrintType = x.OubPrintType,
                    Verdict = x.OubVerdict,
                    InstructionStatus = x.OubInstructionStatus,
                    LabelStatus = x.OubLabelStatus,
                    ApprovedBy = x.OubApprovedBy,
                    DateApproved = x.OubDateApproved
                };
            }
            
            if (x.CbInstructionCode != null && !string.IsNullOrEmpty(x.CbInstructionCode))
            {
                details[BoxType.CartonBox] = new BoxLabelInstructionDetails
                {
                    InstructionCode = x.CbInstructionCode,
                    TargetPrintQuantity = x.CbTargetPrintQuantity,
                    PrintType = x.CbPrintType,
                    Verdict = x.CbVerdict,
                    InstructionStatus = x.CbInstructionStatus,
                    LabelStatus = x.CbLabelStatus,
                    ApprovedBy = x.CbApprovedBy,
                    DateApproved = x.CbDateApproved
                };
            }

            if (x.AcbInstructionCode != null && !string.IsNullOrEmpty(x.AcbInstructionCode))
            {
                details[BoxType.AdditionalCartonBox] = new BoxLabelInstructionDetails
                {
                    InstructionCode = x.AcbInstructionCode,
                    TargetPrintQuantity = x.AcbTargetPrintQuantity,
                    PrintType = x.AcbPrintType,
                    Verdict = x.AcbVerdict,
                    InstructionStatus = x.AcbInstructionStatus,
                    LabelStatus = x.AcbLabelStatus,
                    ApprovedBy = x.AcbApprovedBy,
                    DateApproved = x.AcbDateApproved
                };
            }

            if (x.OcbInstructionCode != null && !string.IsNullOrEmpty(x.OcbInstructionCode))
            {
                details[BoxType.OuterCartonBox] = new BoxLabelInstructionDetails
                {
                    InstructionCode = x.OcbInstructionCode,
                    TargetPrintQuantity = x.OcbTargetPrintQuantity,
                    PrintType = x.OcbPrintType,
                    Verdict = x.OcbVerdict,
                    InstructionStatus = x.OcbInstructionStatus,
                    LabelStatus = x.OcbLabelStatus,
                    ApprovedBy = x.OcbApprovedBy,
                    DateApproved = x.OcbDateApproved
                };
            }

            return details;
        }
    }
}
