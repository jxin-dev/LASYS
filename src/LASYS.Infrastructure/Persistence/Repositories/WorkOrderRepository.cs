using LASYS.Application.Interfaces.Persistence;
using LASYS.Application.Interfaces.Persistence.Repositories;

namespace LASYS.Infrastructure.Persistence.Repositories
{
    public class WorkOrderRepository : IWorkOrderRepository
    {
        private readonly IDbConnectionFactory _factory;

        public WorkOrderRepository(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        //Private ReadOnly CHECKUSERSECTION As String = "" & _
        //    "SELECT section_assignments FROM pre_tpc_products_tcl  " & _
        //    "WHERE section_assignments like '%{0}%' " & _
        //    "   AND item_code = @item_code " & _
        //    "   AND MASTERLABEL_REVISION_NUMBER = @MASTERLABEL_REVISION_NUMBER "


        //Public Shared ReadOnly WORKLOADDATA As String = "" & _
        //" SELECT inspln.Item_Code AS 'ItemCode'," & _
        //"       inspln.Lot_No AS 'Lot No', " & _
        //"   IF (mstlbltcl.UDI = 1, " & _
        //"           date_format(inspln.EXPIRY_DATE, '%Y-%m-%d'), " & _
        //"           date_format(inspln.EXPIRY_DATE, '%Y-%m') " & _
        //"       ) AS 'Exp. Date', " & _
        //"       CASE CASE WHEN ((UB_LBL_INS_PRNT_TYPE IS NULL OR UB_LBL_INS_PRNT_TYPE = '') AND (OUB_LBL_INS_PRNT_TYPE IS NULL OR OUB_LBL_INS_PRNT_TYPE = '') AND (OCB_LBL_INS_PRNT_TYPE IS NULL OR OCB_LBL_INS_PRNT_TYPE = '')) THEN CB_LBL_INS_PRNT_TYPE WHEN ((UB_LBL_INS_PRNT_TYPE IS NULL OR UB_LBL_INS_PRNT_TYPE = '') AND (CB_LBL_INS_PRNT_TYPE IS NULL OR CB_LBL_INS_PRNT_TYPE = '') AND (OCB_LBL_INS_PRNT_TYPE IS NULL OR OCB_LBL_INS_PRNT_TYPE = '')) THEN OUB_LBL_INS_PRNT_TYPE WHEN ((UB_LBL_INS_PRNT_TYPE IS NULL OR UB_LBL_INS_PRNT_TYPE = '') AND (CB_LBL_INS_PRNT_TYPE IS NULL OR CB_LBL_INS_PRNT_TYPE = '') AND (OUB_LBL_INS_PRNT_TYPE IS NULL OR OUB_LBL_INS_PRNT_TYPE = '')) THEN OCB_LBL_INS_PRNT_TYPE ELSE UB_LBL_INS_PRNT_TYPE END " & _
        //"          WHEN 'Returned' " & _
        //"          THEN 'Return' " & _
        //"   ELSE " & _
        //" CASE WHEN ((UB_LBL_INS_PRNT_TYPE IS NULL OR UB_LBL_INS_PRNT_TYPE = '') AND (OUB_LBL_INS_PRNT_TYPE IS NULL OR OUB_LBL_INS_PRNT_TYPE = '') AND (OCB_LBL_INS_PRNT_TYPE IS NULL OR OCB_LBL_INS_PRNT_TYPE = '')) THEN CB_LBL_INS_PRNT_TYPE WHEN ((UB_LBL_INS_PRNT_TYPE IS NULL OR UB_LBL_INS_PRNT_TYPE = '') AND (CB_LBL_INS_PRNT_TYPE IS NULL OR CB_LBL_INS_PRNT_TYPE = '') AND (OCB_LBL_INS_PRNT_TYPE IS NULL OR OCB_LBL_INS_PRNT_TYPE = '')) THEN OUB_LBL_INS_PRNT_TYPE WHEN ((UB_LBL_INS_PRNT_TYPE IS NULL OR UB_LBL_INS_PRNT_TYPE = '') AND (CB_LBL_INS_PRNT_TYPE IS NULL OR CB_LBL_INS_PRNT_TYPE = '') AND (OUB_LBL_INS_PRNT_TYPE IS NULL OR OUB_LBL_INS_PRNT_TYPE = '')) THEN OCB_LBL_INS_PRNT_TYPE ELSE UB_LBL_INS_PRNT_TYPE END " & _
        //"       END " & _
        //"          AS 'Print Type', " & _
        //"(CASE WHEN ((UB_LBL_INS_VERDICT IS NULL OR UB_LBL_INS_VERDICT = '') AND (OUB_LBL_INS_VERDICT IS NULL OR OUB_LBL_INS_VERDICT = '') AND (OCB_LBL_INS_VERDICT IS NULL OR OCB_LBL_INS_VERDICT = '')) THEN CB_LBL_INS_VERDICT WHEN ((UB_LBL_INS_VERDICT IS NULL OR UB_LBL_INS_VERDICT = '') AND (CB_LBL_INS_VERDICT IS NULL OR CB_LBL_INS_VERDICT = '') AND (OCB_LBL_INS_VERDICT IS NULL OR OCB_LBL_INS_VERDICT = '')) THEN OUB_LBL_INS_VERDICT WHEN ((UB_LBL_INS_VERDICT IS NULL OR UB_LBL_INS_VERDICT = '') AND (CB_LBL_INS_VERDICT IS NULL OR CB_LBL_INS_VERDICT = '') AND (OUB_LBL_INS_VERDICT IS NULL OR OUB_LBL_INS_VERDICT = '')) THEN OCB_LBL_INS_VERDICT ELSE UB_LBL_INS_VERDICT END) " & _
        //"          AS 'Verdict'," & _
        //"       Date_format(inspln.UB_LBL_INS_APPRVD_DATETIME, '%m/%d/%Y') " & _
        //"          AS 'Date Approved', " & _
        //"       inspln.Target_Production_Quantity AS 'Prod Qty', " & _
        //"       inspln.master_label_revision_number AS 'Master Label Revision No.', " & _
        //"       inspln.label_ins_Rev_number AS 'Label Ins. Revision No.', " & _
        //"       inspln.UB_LBL_INS_CODE AS 'UB_LI_Code', " & _
        //"       inspln.UB_LBL_INS_TRGT_PRNT_QTY AS 'UB_Qty', " & _
        //"       inspln.UB_LBL_INS_STATUS AS 'UB_LI_Status'," & _
        //"       inspln.AUB_LBL_INS_CODE AS 'AUB_LI_Code', " & _
        //"       inspln.AUB_LBL_INS_TRGT_PRNT_QTY AS 'AUB_Qty', " & _
        //"       inspln.AUB_LBL_INS_STATUS AS 'AUB_LI_Status'," & _
        //"       inspln.OUB_LBL_INS_CODE AS 'OUB_LI_Code', " & _
        //"       inspln.OUB_LBL_INS_TRGT_PRNT_QTY AS 'OUB_Qty', " & _
        //"       inspln.OUB_LBL_INS_STATUS AS 'OUB_LI_Status'," & _
        //"       inspln.CB_LBL_INS_CODE AS 'CB_LI_Code', " & _
        //"       inspln.CB_LBL_INS_TRGT_PRNT_QTY AS 'CB_Qty', " & _
        //"       inspln.CB_LBL_INS_STATUS AS 'CB_LI_Status', " & _
        //"       inspln.ACB_LBL_INS_CODE AS 'ACB_LI_Code', " & _
        //"       inspln.ACB_LBL_INS_TRGT_PRNT_QTY AS 'ACB_Qty', " & _
        //"       inspln.ACB_LBL_INS_STATUS AS 'ACB_LI_Status', " & _
        //"       inspln.OCB_LBL_INS_CODE AS 'OCB_LI_Code', " & _
        //"       inspln.OCB_LBL_INS_TRGT_PRNT_QTY AS 'OCB_Qty', " & _
        //"       inspln.OCB_LBL_INS_STATUS AS 'OCB_LI_Status', " & _
        //"   IF (prdct.LABEL_TYPE_EUMDR_FLAG = true, " & _
        //"           date_format(inspln.MANUFACTURE_DATE, '%Y-%m-%d'), " & _
        //"           null " & _
        //"       ) AS 'Manufacture Date' " & _
        //"  FROM ppt_lbl_instructn_plns_hst inspln " & _
        //"       LEFT JOIN pre_tpc_products_mst prdmst " & _
        //"          ON inspln.item_code = prdmst.item_code " & _
        //"       INNER JOIN pre_masterlabels_tcl  mstlbltcl  " & _
        //"          ON (mstlbltcl.item_code  = inspln.item_code AND " & _
        //"               mstlbltcl.masterlabel_revision_number = inspln.master_label_revision_number) " & _
        //"       INNER JOIN pre_tpc_products_tcl prdct " & _
        //"          ON (inspln.item_code = prdct.item_code " & _
        //" AND prdct.masterlabel_revision_number = mstlbltcl.masterlabel_revision_number) " & _
        //"       INNER JOIN (SELECT item_code, " & _
        //"                                               lot_no," & _
        //"                          MAX(label_ins_rev_number) AS 'max_ins_rev' " & _
        //"                     FROM ppt_lbl_instructn_plns_hst " & _
        //"                   GROUP BY item_code, lot_no) finalinspln" & _
        //"          ON (    finalinspln.item_code = inspln.item_code " & _
        //"              AND finalinspln.lot_no = inspln.lot_no " & _
        //"              AND finalinspln.max_ins_rev = inspln.label_ins_rev_number) {0} " & _
        //" WHERE     1 = 1 " & _
        //    "AND inspln.item_code=prdct.item_code " & _
        //"       AND prdct.masterlabel_revision_number = " & _
        //"              inspln.master_label_revision_number " & _
        //    "AND prdct.active_flag='' " & _
        //"       {1} {2} " & _
        //" ORDER BY inspln.history_datetime DESC {3} "
    }
}
