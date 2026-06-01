SELECT
    i.item_code,
    i.lot_no,
    i.expiry_date,

    i.cb_lbl_ins_code AS ins_code,
    i.cb_lbl_ins_trgt_prnt_qty AS targetqty,

    m.cb_label_file_path AS path,
    m.cb_label_file AS cb_label_file,

    IFNULL(stats.Total_Passed, 0) AS Total_Passed,
    IFNULL(stats.Total_Failed, 0) AS Total_Failed,
    IFNULL(stats.Total_Sample, 0) AS Total_Sample,
    IFNULL(stats.Total_Printed, 0) AS Total_Printed,
    IFNULL(stats.Next_Sequence, 1) AS Next_Sequence,
    IFNULL(stats.Batch_Number, 1) AS Batch_Number,

    i.UB_LBL_INS_STATUS + 0 AS UB_LI_Status,
    i.AUB_LBL_INS_STATUS + 0 AS AUB_LI_Status,
    i.OUB_LBL_INS_STATUS + 0 AS OUB_LI_Status,
    i.CB_LBL_INS_STATUS + 0 AS CB_LI_Status,
    i.ACB_LBL_INS_STATUS + 0 AS ACB_LI_Status,
    i.OCB_LBL_INS_STATUS + 0 AS OCB_LI_Status

FROM ppt_lbl_instructn_plns_hst i

LEFT JOIN
(
    SELECT
        ITEM_CODE,
        cb_label_file_path,
        cb_label_file
    FROM pre_masterlabels_tcl
    WHERE item_code = 'ED-03A10'
      AND masterlabel_revision_number =
      (
            SELECT MAX(MASTERLABEL_REVISION_NUMBER)
            FROM pre_masterlabels_tcl
            WHERE item_code = 'ED-03A10'
              AND DATE_FORMAT(masterlabel_effectivity_date, '%Y-%m-%d') <= DATE('2019-09-30')
              AND MASTERLABEL_STATUS = 'Approved'
      )
) m
ON i.item_code = m.item_code

LEFT JOIN
(
    SELECT
        item_code,
        lot_no,

        COUNT(CASE WHEN label_status IN (3,4,5,12) THEN 1 END) AS Total_Passed,

        COUNT(CASE WHEN label_status = 'Failed During Printing' THEN 1 END) AS Total_Failed,

        COUNT(CASE WHEN label_status IN ('First', 'Last') THEN 1 END) AS Total_Sample,

        COUNT(CASE WHEN label_status IN (1,2,3,4,5,6,7,12) THEN 1 END) AS Total_Printed,

        IFNULL(MAX(SEQUENCE_NUMBER) + 1, 1) AS Next_Sequence,

        IFNULL(
            MAX(
                CASE
                    WHEN label_status IN (
                        'Failed During Printing',
                        'Failed After Printing',
                        'First',
                        'Last'
                    )
                    THEN batch_number
                END
            ),
            1
        ) AS Batch_Number

    FROM prdprnt_cb_labels_tcl
    WHERE item_code = 'ED-03A10'
      AND lot_no = '190930X'
    GROUP BY item_code, lot_no
) stats
ON i.item_code = stats.item_code
AND i.lot_no = stats.lot_no

WHERE
    i.item_code = 'ED-03A10'
    AND i.cb_lbl_ins_code = '91054987892109179201010110190930X'
    AND i.lot_no = '190930X'
    AND i.cb_lbl_ins_verdict = 'Approved'

    AND CONCAT(i.history_datetime, i.item_code, i.lot_no) =
    (
        SELECT CONCAT(MAX(history_datetime), item_code, lot_no)
        FROM ppt_lbl_instructn_plns_hst
        WHERE item_code = i.item_code
          AND lot_no = i.lot_no
    );