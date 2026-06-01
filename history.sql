SELECT * FROM ppt_lbl_instructn_plns_hst ;

SELECT 
	t1.ITEM_CODE AS ItemCode,
    t1.LOT_NO AS LotNo,
    p.UDI,
    t1.EXPIRY_DATE AS ExpirationDate,
    t1.LABEL_INS_REV_NUMBER,
    p.MASTERLABEL_REVISION_NUMBER,
    pr.MASTERLABEL_REVISION_NUMBER
FROM ppt_lbl_instructn_plns_hst t1
LEFT JOIN ppt_lbl_instructn_plns_hst t2
    ON t1.ITEM_CODE = t2.ITEM_CODE
    AND t1.LOT_NO = t2.LOT_NO
    AND t1.LABEL_INS_REV_NUMBER < t2.LABEL_INS_REV_NUMBER
INNER JOIN pre_masterlabels_tcl p
    ON p.ITEM_CODE = t1.ITEM_CODE
    AND p.MASTERLABEL_REVISION_NUMBER = t1.LABEL_INS_REV_NUMBER
INNER JOIN pre_tpc_products_tcl pr
    ON pr.ITEM_CODE = t1.ITEM_CODE
    AND pr.MASTERLABEL_REVISION_NUMBER = t1.LABEL_INS_REV_NUMBER
WHERE t2.ITEM_CODE IS NULL 
AND ((t1.UB_LBL_INS_STATUS IN ('For Printing','Printed') AND t1.UB_LBL_INS_VERDICT = 'Approved' AND t1.UB_Lbl_Status IN ('Not Printed','Partially Printed','Completely Printed')) OR
	(t1.OUB_LBL_INS_STATUS IN ('For Printing','Printed') AND t1.OUB_LBL_INS_VERDICT = 'Approved' AND t1.OUB_Lbl_Status IN ('Not Printed','Partially Printed','Completely Printed')) OR
	(t1.OCB_LBL_INS_STATUS IN ('For Printing','Printed') AND t1.OCB_LBL_INS_VERDICT = 'Approved' AND t1.OCB_Lbl_Status IN ('Not Printed','Partially Printed','Completely Printed')) OR
	(t1.CB_LBL_INS_STATUS IN ('For Printing','Printed') AND t1.CB_LBL_INS_VERDICT = 'Approved' AND t1.CB_Lbl_Status IN ('Not Printed','Partially Printed','Completely Printed')))
AND pr.ACTIVE_FLAG IS NOT NULL
AND t1.ITEM_CODE = 'NN-2070C' AND t1.LOT_NO = '251022A';



CREATE INDEX idx_hst_item_lot_rev
ON ppt_lbl_instructn_plns_hst(ITEM_CODE, LOT_NO, LABEL_INS_REV_NUMBER);

CREATE INDEX idx_pre_item_rev
ON pre_masterlabels_tcl(ITEM_CODE, MASTERLABEL_REVISION_NUMBER);


CREATE INDEX idx_product_item
ON pre_tpc_products_tcl(ITEM_CODE, MASTERLABEL_REVISION_NUMBER);

SHOW INDEX FROM ppt_lbl_instructn_plns_hst;
SHOW INDEX FROM pre_masterlabels_tcl;
SHOW INDEX FROM pre_tpc_products_tcl;

