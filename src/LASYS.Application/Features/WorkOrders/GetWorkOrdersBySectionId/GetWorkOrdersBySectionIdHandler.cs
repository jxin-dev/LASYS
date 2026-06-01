using Dapper;
using LASYS.Application.Common.Pagination;
using LASYS.Application.Common.Results;
using LASYS.Application.Interfaces.Context;
using LASYS.Application.Interfaces.Persistence;
using MediatR;

namespace LASYS.Application.Features.WorkOrders.GetWorkOrdersBySectionId
{
    public sealed class GetWorkOrdersBySectionIdHandler : IRequestHandler<GetWorkOrdersBySectionIdQuery, Result<PaginatedList<Product>>>
    {
        private readonly IDbConnectionFactory _factory;
        private readonly ICurrentUser _currentUser;
        public GetWorkOrdersBySectionIdHandler(IDbConnectionFactory factory, ICurrentUser currentUser)
        {
            _factory = factory;
            _currentUser = currentUser;
        }

        public async Task<Result<PaginatedList<Product>>> Handle(GetWorkOrdersBySectionIdQuery request, CancellationToken cancellationToken)
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
                t1.LINE_CODE AS LineCode,
                pr.SECTION_ASSIGNMENTS AS SectionAssignments,
	            t1.ITEM_CODE AS ItemCode,
                t1.LOT_NO AS LotNo,
                t1.LABEL_INS_REV_NUMBER AS LabelInsRevNumber,
                p.MASTERLABEL_REVISION_NUMBER AS MasterLabelRevNumber,
                p.UDI AS Udi,
                pr.LABEL_TYPE_EUMDR_FLAG AS IsEumdr,    
                IF(
                    pr.LABEL_TYPE_EUMDR_FLAG = 1 AND t1.MANUFACTURE_DATE IS NOT NULL,
                    1,
                    0
                ) AS IsEumdr,
                IF(
                    pr.LABEL_TYPE_EUMDR_FLAG = 1 AND t1.MANUFACTURE_DATE IS NOT NULL,
                    t1.MANUFACTURE_DATE,
                    NULL
                ) AS ManufactureDate,
                t1.EXPIRY_DATE AS ExpirationDate,
                t1.PRODUCTION_DATE AS ProductionDate,
                t1.STERILIZATION_DATE AS SterilizationDate,
                t1.Target_Production_Quantity AS TargetProductionQuantity,
                
                -- pr.ITEM_NAME AS ItemName,
                -- pr.ITEM_GROUP_TYPE_CODE AS ItemGroupTypeCode,
                -- pr.MARKET_CODE AS MarketCode,
                -- pr.ITEM_TYPE AS ItemType,
                -- pr.CALENDAR_TYPE AS CalendarType,
                -- pr.DESCRIPTION AS Description,
                -- pr.TIP_TYPE AS TipType,
                -- pr.WITH_COC AS WithCoc,
                -- pr.WITH_STERILIZATION AS WithSterilization,
                -- pr.BARCODE_TYPE AS BarcodeType,
                -- pr.BARCODE_CATEGORY AS BarcodeCategory,
                -- pr.UB_PER_CB_QUANTITY AS UbPerCbQuantity,
                -- pr.CB_PER_PALLETE AS CbPerPallete,
                -- pr.QC_SAMPLE_QUANTITY AS QcSampleQuantity,
                -- pr.SB_QUANTITY AS SbQuantity,
                -- pr.PQE_CONTROL_NUMBER_CODE AS PqeControlNumberCode,
                -- pr.DVR_NUMBER AS DvrNumber,
                -- pr.DEPKES_NUMBER AS DepkesNumber,
                -- pr.CE_MARK AS CeMark,
                -- pr.SR_TYPE AS SrType,
                -- pr.GAUGE AS Gauge,
                -- pr.NEEDLE_GAUGE AS NeedleGauge,
                -- pr.NEEDLE_SIZE AS NeedleSize,
                -- pr.TRANSFER_INSTRUCTION AS TransferInstruction,
                -- pr.PSS_ID_NO AS PssIdNo,
                -- pr.FLOW_RATE AS FlowRate,
                -- pr.DESCRIPTION_1 AS Description1,
                -- pr.DESCRIPTION_2 AS Description2,
                -- pr.Custom_1 AS Custom1,
                -- pr.Custom_2 AS Custom2,
                -- pr.Custom_3 AS Custom3,
                -- pr.Custom_4 AS Custom4,
                -- pr.Custom_5 AS Custom5,
                -- pr.Custom_6 AS Custom6,
                -- pr.Custom_7 AS Custom7,
                -- pr.Custom_8 AS Custom8,
                -- pr.Custom_9 AS Custom9,
                -- pr.Custom_10 AS Custom10,
                
                t1.UB_LBL_INS_CODE AS UbInstructionCode,
                t1.UB_LBL_INS_TRGT_PRNT_QTY AS UbTargetPrintQuantity,
                t1.UB_LBL_INS_PRNT_TYPE AS UbPrintType, 
                t1.UB_LBL_INS_VERDICT AS UbVerdict,
                t1.UB_LBL_INS_STATUS AS UbInstructionStatus,
                t1.UB_LBL_STATUS AS UbLabelStatus,
                t1.UB_LBL_INS_APPRVD_USR_CD AS UbApprovedBy,
                STR_TO_DATE(t1.UB_LBL_INS_APPRVD_DATETIME,  '%Y%m%d%H%i%s') AS UbDateApproved,
                pr.UB_REPORT_TYPE_CODE AS UbReportTypeCode,
                pr.UB_BARCODE_NUMBER AS UbBarcodeNumber,
                pr.UB_QUANTITY AS UbQuantity,
                p.UB_LABEL_FILE_PATH AS UbNiceLabelPath,

                t1.AUB_LBL_INS_CODE AS AubInstructionCode,
                t1.AUB_LBL_INS_TRGT_PRNT_QTY AS AubTargetPrintQuantity,
                t1.AUB_LBL_INS_PRNT_TYPE AS AubPrintType, 
                t1.AUB_LBL_INS_VERDICT AS AubVerdict,
                t1.AUB_LBL_INS_STATUS AS AubInstructionStatus,
                t1.AUB_LBL_STATUS AS AubLabelStatus,
                t1.AUB_LBL_INS_APPRVD_USR_CD AS AubApprovedBy,
                STR_TO_DATE(t1.AUB_LBL_INS_APPRVD_DATETIME,  '%Y%m%d%H%i%s') AS AubDateApproved,
                pr.AUB_REPORT_TYPE_CODE AS AubReportTypeCode,
                pr.AUB_BARCODE_NUMBER AS AubBarcodeNumber,
                pr.AUB_QUANTITY AS AubQuantity,
                p.AUB_LABEL_FILE_PATH AS AubNiceLabelPath,

                t1.OUB_LBL_INS_CODE AS OubInstructionCode,
                t1.OUB_LBL_INS_TRGT_PRNT_QTY AS OubTargetPrintQuantity,
                t1.OUB_LBL_INS_PRNT_TYPE AS OubPrintType, 
                t1.OUB_LBL_INS_VERDICT AS OubVerdict,
                t1.OUB_LBL_INS_STATUS AS OubInstructionStatus,
                t1.OUB_LBL_STATUS AS OubLabelStatus,
                t1.OUB_LBL_INS_APPRVD_USR_CD AS OubApprovedBy,
                STR_TO_DATE(t1.OUB_LBL_INS_APPRVD_DATETIME,  '%Y%m%d%H%i%s') AS OubDateApproved,
                pr.OUB_REPORT_TYPE_CODE AS OubReportTypeCode,
                pr.OUB_BARCODE_NUMBER AS OubBarcodeNumber,
                pr.OUB_QUANTITY AS OubQuantity,
                p.OUB_LABEL_FILE_PATH AS OubNiceLabelPath,

                t1.CB_LBL_INS_CODE AS CbInstructionCode,
                t1.CB_LBL_INS_TRGT_PRNT_QTY AS CbTargetPrintQuantity,
                t1.CB_LBL_INS_PRNT_TYPE AS CbPrintType, 
                t1.CB_LBL_INS_VERDICT AS CbVerdict,
                t1.CB_LBL_INS_STATUS AS CbInstructionStatus,
                t1.CB_LBL_STATUS AS CbLabelStatus,
                t1.CB_LBL_INS_APPRVD_USR_CD AS CbApprovedBy,
                STR_TO_DATE(t1.CB_LBL_INS_APPRVD_DATETIME,  '%Y%m%d%H%i%s') AS CbDateApproved,
                pr.CB_REPORT_TYPE_CODE AS CbReportTypeCode,
                pr.CB_BARCODE_NUMBER AS CbBarcodeNumber,
                pr.CB_QUANTITY AS CbQuantity,
                p.CB_LABEL_FILE_PATH AS CbNiceLabelPath,

                t1.ACB_LBL_INS_CODE AS AcbInstructionCode,
                t1.ACB_LBL_INS_TRGT_PRNT_QTY AS AcbTargetPrintQuantity,
                t1.ACB_LBL_INS_PRNT_TYPE AS AcbPrintType, 
                t1.ACB_LBL_INS_VERDICT AS AcbVerdict,
                t1.ACB_LBL_INS_STATUS AS AcbInstructionStatus,
                t1.ACB_LBL_STATUS AS AcbLabelStatus,
                t1.ACB_LBL_INS_APPRVD_USR_CD AS AcbApprovedBy,
                STR_TO_DATE(t1.ACB_LBL_INS_APPRVD_DATETIME,  '%Y%m%d%H%i%s') AS AcbDateApproved,
                pr.ACB_REPORT_TYPE_CODE AS AcbReportTypeCode,
                pr.ACB_BARCODE_NUMBER AS AcbBarcodeNumber,
                pr.ACB_QUANTITY AS AcbQuantity,
                p.ACB_LABEL_FILE_PATH AS AcbNiceLabelPath,

                t1.OCB_LBL_INS_CODE AS OcbInstructionCode,
                t1.OCB_LBL_INS_TRGT_PRNT_QTY AS OcbTargetPrintQuantity,
                t1.OCB_LBL_INS_PRNT_TYPE AS OcbPrintType, 
                t1.OCB_LBL_INS_VERDICT AS OcbVerdict,
                t1.OCB_LBL_INS_STATUS AS OcbInstructionStatus,
                t1.OCB_LBL_STATUS AS OcbLabelStatus,
                t1.OCB_LBL_INS_APPRVD_USR_CD AS OcbApprovedBy,
                STR_TO_DATE(t1.OCB_LBL_INS_APPRVD_DATETIME,  '%Y%m%d%H%i%s') AS OcbDateApproved,
                pr.OCB_REPORT_TYPE_CODE AS OcbReportTypeCode,
                pr.OCB_BARCODE_NUMBER AS OcbBarcodeNumber,
                pr.OCB_QUANTITY AS OcbQuantity,
                p.OCB_LABEL_FILE_PATH AS OcbNiceLabelPath

            FROM ppt_lbl_instructn_plns_hst t1
            LEFT JOIN ppt_lbl_instructn_plns_hst t2
                ON t1.ITEM_CODE = t2.ITEM_CODE
                AND t1.LOT_NO = t2.LOT_NO
	            -- AND t1.LABEL_INS_REV_NUMBER < t2.LABEL_INS_REV_NUMBER
                AND t1.MASTER_LABEL_REVISION_NUMBER < t2.MASTER_LABEL_REVISION_NUMBER
            INNER JOIN pre_masterlabels_tcl p
                ON p.ITEM_CODE = t1.ITEM_CODE
                -- AND p.MASTERLABEL_REVISION_NUMBER = t1.LABEL_INS_REV_NUMBER
                AND p.MASTERLABEL_REVISION_NUMBER = t1.MASTER_LABEL_REVISION_NUMBER
            INNER JOIN pre_tpc_products_tcl pr
                ON pr.ITEM_CODE = t1.ITEM_CODE
                -- AND pr.MASTERLABEL_REVISION_NUMBER = t1.LABEL_INS_REV_NUMBER
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
            using var connection = await _factory.CreateConnectionAsync();
            try
            {
                var rows = await connection.QueryAsync(sql, new { SectionId = sectionId });

                var list = rows.Select(x => new Product
                {
                    AvailableBoxTypes = x.AvailableBoxTypes,
                    LineSectionAssignment = new LineSectionAssignment
                    {
                        LineCode = x.LineCode
                    }.WithSectionIds(x.SectionAssignments),

                    ItemCode = x.ItemCode,
                    LotNo = x.LotNo,
                    LabelInsRevNumber = x.LabelInsRevNumber,
                    MasterLabelRevNumber = x.MasterLabelRevNumber,
                    Udi = x.Udi,
                    IsEumdr = Convert.ToBoolean(x.IsEumdr),
                    ManufactureDate = x.ManufactureDate,
                    ExpirationDate = x.ExpirationDate,
                    ProductionDate = x.ProductionDate,
                    SterilizationDate = x.SterilizationDate,
                    TargetProductionQuantity = x.TargetProductionQuantity,

                    UbLabel = x.UbInstructionCode is null
                        ? null
                        : new LabelDetails(
                            x.UbInstructionCode,
                            x.UbTargetPrintQuantity,
                            x.UbPrintType,
                            x.UbVerdict,
                            x.UbInstructionStatus,
                            x.UbLabelStatus,
                            x.UbApprovedBy,
                            x.UbDateApproved,
                            x.UbReportType,
                            x.UbBarcodeNumber,
                            x.UbQuantity),
                    AubLabel = x.AubInstructionCode is null
                        ? null
                        : new LabelDetails(
                            x.AubInstructionCode,
                            x.AubTargetPrintQuantity,
                            x.AubPrintType,
                            x.AubVerdict,
                            x.AubInstructionStatus,
                            x.AubLabelStatus,
                            x.AubApprovedBy,
                            x.AubDateApproved,
                            x.AubReportType,
                            x.AubBarcodeNumber,
                            x.AubQuantity),
                    OubLabel = x.OubInstructionCode is null
                        ? null
                        : new LabelDetails(
                            x.OubInstructionCode,
                            x.OubTargetPrintQuantity,
                            x.OubPrintType,
                            x.OubVerdict,
                            x.OubInstructionStatus,
                            x.OubLabelStatus,
                            x.OubApprovedBy,
                            x.OubDateApproved,
                            x.OubReportType,
                            x.OubBarcodeNumber,
                            x.OubQuantity),
                    CbLabel = x.CbInstructionCode is null
                        ? null
                        : new LabelDetails(
                            x.CbInstructionCode,
                            x.CbTargetPrintQuantity,
                            x.CbPrintType,
                            x.CbVerdict,
                            x.CbInstructionStatus,
                            x.CbLabelStatus,
                            x.CbApprovedBy,
                            x.CbDateApproved,
                            x.CbReportType,
                            x.CbBarcodeNumber,
                            x.CbQuantity),
                    AcbLabel = x.AcbInstructionCode is null
                        ? null
                        : new LabelDetails(
                            x.AcbInstructionCode,
                            x.AcbTargetPrintQuantity,
                            x.AcbPrintType,
                            x.AcbVerdict,
                            x.AcbInstructionStatus,
                            x.AcbLabelStatus,
                            x.AcbApprovedBy,
                            x.AcbDateApproved,
                            x.AcbReportType,
                            x.AcbBarcodeNumber,
                            x.AcbQuantity),
                    OcbLabel = x.OcbInstructionCode is null
                        ? null
                        : new LabelDetails(
                            x.OcbInstructionCode,
                            x.OcbTargetPrintQuantity,
                            x.OcbPrintType,
                            x.OcbVerdict,
                            x.OcbInstructionStatus,
                            x.OcbLabelStatus,
                            x.OcbApprovedBy,
                            x.OcbDateApproved,
                            x.OcbReportType,
                            x.OcbBarcodeNumber,
                            x.OcbQuantity)

                }).ToList();

                if (list is null || !list.Any())
                {
                    return Result.Failure<PaginatedList<Product>>("No work orders found for the specified section.");
                }
                var items = new PaginatedList<Product>(list, list.Count, 1, 10);
                return Result.Success(items);

            }
            catch (Exception ex)
            {
                return Result.Failure<PaginatedList<Product>>("No work orders found for the specified section.");
            }
          
        }
    }
}
