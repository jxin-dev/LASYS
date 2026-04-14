using Dapper;
using LASYS.Application.Common.Results;
using LASYS.Application.Interfaces.Persistence;
using MediatR;

namespace LASYS.Application.Features.LabelProcessing.GetWorkOrders
{
    public class GetWorkOrdersHandler : IRequestHandler<GetWorkOrdersQuery, Result<IEnumerable<GetWorkOrdersResult>>>
    {
        private readonly IDbConnectionFactory _factory;
        public GetWorkOrdersHandler(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<Result<IEnumerable<GetWorkOrdersResult>>> Handle(GetWorkOrdersQuery request, CancellationToken cancellationToken)
        {
            try
            {
                using var connection = await _factory.CreateConnectionAsync();

                var filter = $@"
                    inspln.Item_Code LIKE '%{request.filter}%' OR
                    inspln.Lot_No LIKE '%{request.filter}%' OR
                    inspln.UB_LBL_INS_CODE LIKE '%{request.filter}%' OR
                    inspln.OUB_LBL_INS_CODE LIKE '%{request.filter}%' OR    
                    inspln.OCB_LBL_INS_CODE LIKE '%{request.filter}%' OR
                    inspln.CB_LBL_INS_CODE LIKE '%{request.filter}%' OR
                    inspln.AUB_LBL_INS_CODE LIKE '%{request.filter}%' OR
                    inspln.ACB_LBL_INS_CODE LIKE '%{request.filter}%'
                ";

                var query = @$"SELECT inspln.Item_Code AS 'ItemCode',
	                            inspln.Lot_No AS 'LotNo', 
	                            IF (mstlbltcl.UDI = 1, 
	                               date_format(inspln.EXPIRY_DATE, '%Y-%m-%d'), 
	                               date_format(inspln.EXPIRY_DATE, '%Y-%m') 
	                            ) AS 'ExpDate', 
                            CASE CASE WHEN ((UB_LBL_INS_PRNT_TYPE IS NULL OR UB_LBL_INS_PRNT_TYPE = '') AND (OUB_LBL_INS_PRNT_TYPE IS NULL OR OUB_LBL_INS_PRNT_TYPE = '') AND (OCB_LBL_INS_PRNT_TYPE IS NULL OR OCB_LBL_INS_PRNT_TYPE = '')) THEN CB_LBL_INS_PRNT_TYPE WHEN ((UB_LBL_INS_PRNT_TYPE IS NULL OR UB_LBL_INS_PRNT_TYPE = '') AND (CB_LBL_INS_PRNT_TYPE IS NULL OR CB_LBL_INS_PRNT_TYPE = '') AND (OCB_LBL_INS_PRNT_TYPE IS NULL OR OCB_LBL_INS_PRNT_TYPE = '')) THEN OUB_LBL_INS_PRNT_TYPE WHEN ((UB_LBL_INS_PRNT_TYPE IS NULL OR UB_LBL_INS_PRNT_TYPE = '') AND (CB_LBL_INS_PRNT_TYPE IS NULL OR CB_LBL_INS_PRNT_TYPE = '') AND (OUB_LBL_INS_PRNT_TYPE IS NULL OR OUB_LBL_INS_PRNT_TYPE = '')) THEN OCB_LBL_INS_PRNT_TYPE ELSE UB_LBL_INS_PRNT_TYPE END 
                              WHEN 'Returned' THEN 'Return' 
                            ELSE 
	                            CASE WHEN ((UB_LBL_INS_PRNT_TYPE IS NULL OR UB_LBL_INS_PRNT_TYPE = '') AND (OUB_LBL_INS_PRNT_TYPE IS NULL OR OUB_LBL_INS_PRNT_TYPE = '') AND (OCB_LBL_INS_PRNT_TYPE IS NULL OR OCB_LBL_INS_PRNT_TYPE = '')) THEN CB_LBL_INS_PRNT_TYPE WHEN ((UB_LBL_INS_PRNT_TYPE IS NULL OR UB_LBL_INS_PRNT_TYPE = '') AND (CB_LBL_INS_PRNT_TYPE IS NULL OR CB_LBL_INS_PRNT_TYPE = '') AND (OCB_LBL_INS_PRNT_TYPE IS NULL OR OCB_LBL_INS_PRNT_TYPE = '')) THEN OUB_LBL_INS_PRNT_TYPE WHEN ((UB_LBL_INS_PRNT_TYPE IS NULL OR UB_LBL_INS_PRNT_TYPE = '') AND (CB_LBL_INS_PRNT_TYPE IS NULL OR CB_LBL_INS_PRNT_TYPE = '') AND (OUB_LBL_INS_PRNT_TYPE IS NULL OR OUB_LBL_INS_PRNT_TYPE = '')) THEN OCB_LBL_INS_PRNT_TYPE ELSE UB_LBL_INS_PRNT_TYPE END 
                            END AS 'PrintType', 
                            (CASE WHEN ((UB_LBL_INS_VERDICT IS NULL OR UB_LBL_INS_VERDICT = '') AND (OUB_LBL_INS_VERDICT IS NULL OR OUB_LBL_INS_VERDICT = '') AND (OCB_LBL_INS_VERDICT IS NULL OR OCB_LBL_INS_VERDICT = '')) THEN CB_LBL_INS_VERDICT WHEN ((UB_LBL_INS_VERDICT IS NULL OR UB_LBL_INS_VERDICT = '') AND (CB_LBL_INS_VERDICT IS NULL OR CB_LBL_INS_VERDICT = '') AND (OCB_LBL_INS_VERDICT IS NULL OR OCB_LBL_INS_VERDICT = '')) THEN OUB_LBL_INS_VERDICT WHEN ((UB_LBL_INS_VERDICT IS NULL OR UB_LBL_INS_VERDICT = '') AND (CB_LBL_INS_VERDICT IS NULL OR CB_LBL_INS_VERDICT = '') AND (OUB_LBL_INS_VERDICT IS NULL OR OUB_LBL_INS_VERDICT = '')) THEN OCB_LBL_INS_VERDICT ELSE UB_LBL_INS_VERDICT END) AS 'Verdict',
                            Date_format(inspln.UB_LBL_INS_APPRVD_DATETIME, '%m/%d/%Y') AS 'DateApproved', 
                            inspln.Target_Production_Quantity AS 'ProdQty', 
                            inspln.master_label_revision_number AS 'MasterLabelRevisionNo', 
                            inspln.label_ins_Rev_number AS 'LabelInsRevisionNo', 
                            inspln.UB_LBL_INS_CODE AS 'UB_LI_Code', 
                            inspln.UB_LBL_INS_TRGT_PRNT_QTY AS 'UB_Qty', 
                            inspln.UB_LBL_INS_STATUS AS 'UB_LI_Status',
                            inspln.AUB_LBL_INS_CODE AS 'AUB_LI_Code', 
                            inspln.AUB_LBL_INS_TRGT_PRNT_QTY AS 'AUB_Qty', 
                            inspln.AUB_LBL_INS_STATUS AS 'AUB_LI_Status',
                            inspln.OUB_LBL_INS_CODE AS 'OUB_LI_Code', 
                            inspln.OUB_LBL_INS_TRGT_PRNT_QTY AS 'OUB_Qty', 
                            inspln.OUB_LBL_INS_STATUS AS 'OUB_LI_Status',
                            inspln.CB_LBL_INS_CODE AS 'CB_LI_Code', 
                            inspln.CB_LBL_INS_TRGT_PRNT_QTY AS 'CB_Qty', 
                            inspln.CB_LBL_INS_STATUS AS 'CB_LI_Status', 
                            inspln.ACB_LBL_INS_CODE AS 'ACB_LI_Code', 
                            inspln.ACB_LBL_INS_TRGT_PRNT_QTY AS 'ACB_Qty', 
                            inspln.ACB_LBL_INS_STATUS AS 'ACB_LI_Status', 
                            inspln.OCB_LBL_INS_CODE AS 'OCB_LI_Code', 
                            inspln.OCB_LBL_INS_TRGT_PRNT_QTY AS 'OCB_Qty', 
                            inspln.OCB_LBL_INS_STATUS AS 'OCB_LI_Status', 
                            mstlbltcl.IS_CASE_OCR_SUPPORTED AS 'CASE_OCR',
                            mstlbltcl.IS_UB_OCR_SUPPORTED AS 'UB_OCR',
                            mstlbltcl.IS_OUB_OCR_SUPPORTED AS 'OUB_OCR',
                            mstlbltcl.IS_CB_OCR_SUPPORTED AS 'CB_OCR',
                            mstlbltcl.IS_OCB_OCR_SUPPORTED AS 'OCB_OCR',
                            mstlbltcl.IS_AUB_OCR_SUPPORTED AS 'AUB_OCR',
                            mstlbltcl.IS_ACB_OCR_SUPPORTED AS 'ACB_OCR',
                            IF (prdct.LABEL_TYPE_EUMDR_FLAG = true, 
                               date_format(inspln.MANUFACTURE_DATE, '%Y-%m-%d'), 
                               null 
                            ) AS 'ManufactureDate' 
                            FROM ppt_lbl_instructn_plns_hst inspln 
                               LEFT JOIN pre_tpc_products_mst prdmst 
	                              ON inspln.item_code = prdmst.item_code 
                               INNER JOIN pre_masterlabels_tcl  mstlbltcl  
	                              ON (mstlbltcl.item_code  = inspln.item_code AND 
		                               mstlbltcl.masterlabel_revision_number = inspln.master_label_revision_number ) 
                               INNER JOIN pre_tpc_products_tcl prdct 
	                              ON (inspln.item_code = prdct.item_code 
                            AND prdct.masterlabel_revision_number = mstlbltcl.masterlabel_revision_number) 
                            INNER JOIN (SELECT item_code, lot_no, MAX(label_ins_rev_number) AS 'max_ins_rev' FROM ppt_lbl_instructn_plns_hst GROUP BY item_code, lot_no) finalinspln
                            ON (finalinspln.item_code = inspln.item_code 
	                            AND finalinspln.lot_no = inspln.lot_no 
	                            AND finalinspln.max_ins_rev = inspln.label_ins_rev_number)
                            WHERE 1 = 1 
                            AND inspln.item_code=prdct.item_code 
                            AND prdct.masterlabel_revision_number = inspln.master_label_revision_number 
                            AND prdct.active_flag = '' 
                            AND ((inspln.UB_LBL_INS_STATUS IN (2,3) AND inspln.UB_LBL_INS_VERDICT = 2 AND inspln.UB_Lbl_Status IN ('Not Printed','Partially Printed','Completely Printed')) OR  
                            (inspln.OUB_LBL_INS_STATUS IN (2,3) AND inspln.OUB_LBL_INS_VERDICT = 2 AND inspln.OUB_Lbl_Status IN ('Not Printed','Partially Printed','Completely Printed')) OR  
                            (inspln.OCB_LBL_INS_STATUS IN (2,3) AND inspln.OCB_LBL_INS_VERDICT = 2 AND inspln.OCB_Lbl_Status IN ('Not Printed','Partially Printed','Completely Printed')) OR  
                            (inspln.CB_LBL_INS_STATUS IN (2,3) AND inspln.CB_LBL_INS_VERDICT = 2 AND inspln.CB_Lbl_Status IN ('Not Printed','Partially Printed','Completely Printed')) ) 
                            {filter}
                            ORDER BY inspln.history_datetime DESC
                            LIMIT {request.pageSize} OFFSET {request.pageNo * request.pageSize}";

                var result = await connection.QueryAsync<GetWorkOrdersResult>(query);
                return Result<IEnumerable<GetWorkOrdersResult>>.Success(result);
            }
            catch(Exception ex)
            {
                return (Result<IEnumerable<GetWorkOrdersResult>>)Result.Failure(ex.Message);
            }
        }
    }
}
