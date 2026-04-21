// =============================================================================
// LASYS – Label Printing (C# Port)
// File   : WorkOrderRepository.cs
// Purpose: Database access layer – mirrors StatusMonitoringDAL.vb.
//          All SQL is parameterised (no string concatenation with user data).
// =============================================================================

using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using MySql.Data.MySqlClient;   // replace with your actual DB driver reference

namespace LASYS.LabelPrinting
{
    /// <summary>
    /// Thin data-access wrapper for the work-order / instruction-barcode tables.
    /// Mirrors StatusMonitoringDAL.vb (ChkExist, GetItemString, GetList).
    /// </summary>
    public class WorkOrderRepository
    {
        private readonly string _connectionString;

        public WorkOrderRepository(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            _connectionString = connectionString;
        }

        // ── helpers ──────────────────────────────────────────────────────────

        private MySqlConnection OpenConnection()
        {
            var conn = new MySqlConnection(_connectionString);
            conn.Open();
            return conn;
        }

        private static DynamicParameters BuildParameters(MySqlParameter[] parameters)
        {
            var dp = new DynamicParameters();

            if (parameters == null)
                return dp;

            foreach (var p in parameters)
            {
                if (p == null) continue;
                dp.Add(p.ParameterName, p.Value);
            }

            return dp;
        }

        // ── ChkExist ─────────────────────────────────────────────────────────

        /// <summary>
        /// Returns true when the supplied COUNT(*) query returns at least 1 row.
        /// Mirrors StatusMonitoringDAL.ChkExist().
        /// </summary>
        public bool CheckExists(string sql, params MySqlParameter[] parameters)
        {
            using var conn = OpenConnection();
            var scalar = conn.ExecuteScalar<object>(sql, BuildParameters(parameters));
            return scalar != null && Convert.ToInt64(scalar) > 0;
        }

        // ── GetItemString ────────────────────────────────────────────────────

        /// <summary>
        /// Returns the first column of the first row as a string.
        /// Mirrors StatusMonitoringDAL.GetItemString().
        /// </summary>
        public string GetItemString(string sql, params MySqlParameter[] parameters)
        {
            using var conn = OpenConnection();
            var result = conn.ExecuteScalar<object>(sql, BuildParameters(parameters));
            return result?.ToString() ?? string.Empty;
        }

        // ── GetList ──────────────────────────────────────────────────────────

        /// <summary>
        /// Returns a DataTable for the given query.
        /// Mirrors StatusMonitoringDAL.GetList().
        /// </summary>
        public DataTable GetList(string sql, params MySqlParameter[] parameters)
        {
            using var conn    = OpenConnection();
            var table = new DataTable();

            using var reader = conn.ExecuteReader(sql, BuildParameters(parameters));
            table.Load(reader);

            return table;
        }

        // ── Work-order listing ───────────────────────────────────────────────

        private const string GetWorkOrderSql = @"
            SELECT
                item_code                       AS 'ItemCode',
                lot_no                          AS 'LotNo',
                expiry_date                     AS 'ExpiryDate',
                target_production_quantity      AS 'ProdQty',
                ub_lbl_ins_code                 AS 'UB_LI_Code',
                ub_lbl_ins_trgt_prnt_qty        AS 'UB_Qty',
                ub_lbl_ins_prnt_type            AS 'UB_PrintType',
                ub_lbl_ins_verdict              AS 'UB_Verdict',
                ub_lbl_ins_status               AS 'UB_LI_Status',
                IFNULL((
                    SELECT CASE
                        WHEN targetqty = total_printed THEN 'Completely Printed'
                        WHEN total_printed > 0         THEN 'Partially Printed'
                        ELSE 'Not Printed' END
                    FROM (
                        SELECT i.ub_lbl_ins_trgt_prnt_qty AS targetqty,
                               i.item_code, i.lot_no,
                               (SELECT COUNT(*) FROM prdprnt_ub_labels_tcl
                                WHERE item_code = i.item_code
                                  AND lot_no    = i.lot_no
                                  AND label_status IN ('Original','Additional','Replacement','Returned')
                               ) AS total_printed
                        FROM   ppt_lbl_instructn_plns_hst i
                        WHERE  i.item_code = fin.item_code
                          AND  i.lot_no    = fin.lot_no
                    ) ls
                ), 'Not Printed')                AS 'UB_LabelStatus',
                cb_lbl_ins_code                 AS 'CB_LI_Code',
                cb_lbl_ins_trgt_prnt_qty        AS 'CB_Qty',
                cb_lbl_ins_prnt_type            AS 'CB_PrintType',
                cb_lbl_ins_verdict              AS 'CB_Verdict',
                cb_lbl_ins_status               AS 'CB_LI_Status',
                IFNULL((
                    SELECT CASE
                        WHEN targetqty = total_printed THEN 'Completely Printed'
                        WHEN total_printed > 0         THEN 'Partially Printed'
                        ELSE 'Not Printed' END
                    FROM (
                        SELECT i.ub_lbl_ins_trgt_prnt_qty AS targetqty,
                               i.item_code, i.lot_no,
                               (SELECT COUNT(*) FROM prdprnt_cb_labels_tcl
                                WHERE item_code = i.item_code
                                  AND lot_no    = i.lot_no
                                  AND label_status IN ('Original','Additional','Replacement','Returned')
                               ) AS total_printed
                        FROM   ppt_lbl_instructn_plns_hst i
                        WHERE  i.item_code = fin.item_code
                          AND  i.lot_no    = fin.lot_no
                    ) ls
                ), 'Not Printed')                AS 'CB_LabelStatus'
            FROM   ppt_lbl_instructn_plns_hst fin
            WHERE  history_datetime >= DATE_SUB(NOW(), INTERVAL 12 MONTH)
              AND  history_datetime <= NOW()
              AND  (
                       (UB_LBL_INS_STATUS IN (2,3) AND UB_LBL_INS_VERDICT = 2)
                    OR (CB_LBL_INS_STATUS IN (2,3) AND CB_LBL_INS_VERDICT = 2)
                   )
            ORDER BY history_datetime DESC";

        /// <summary>Returns the last 12 months of approved work-order rows.</summary>
        public DataTable GetWorkOrders() => GetList(GetWorkOrderSql);

        // ── Instruction-barcode existence ────────────────────────────────────

        private const string CheckInsBarcodeExistsSql = @"
            SELECT COUNT(*) FROM ppt_lbl_instructn_plns_hst
            WHERE  (ub_lbl_ins_code = @barcode
                 OR cb_lbl_ins_code = @barcode
                 OR oub_lbl_ins_code = @barcode
                 OR aub_lbl_ins_code = @barcode
                 OR acb_lbl_ins_code = @barcode
                 OR ocb_lbl_ins_code = @barcode)
              AND  ub_lbl_ins_status NOT IN (4)"; // exclude Scrapped

        /// <summary>
        /// Returns true if the barcode is found in any instruction-plan column
        /// (excluding scrapped records).  Mirrors PRINTINSBARCODEINFO usage.
        /// </summary>
        public bool CheckInstructionBarcodeExists(string barcode)
        {
            var p = new MySqlParameter("@barcode", barcode);
            return CheckExists(CheckInsBarcodeExistsSql, p);
        }

        // ── Item-code lookup from barcode ─────────────────────────────────────

        private const string GetItemCodeSql = @"
            SELECT item_code FROM ppt_lbl_instructn_plns_hst
            WHERE  (ub_lbl_ins_code = @barcode
                 OR cb_lbl_ins_code = @barcode
                 OR oub_lbl_ins_code = @barcode
                 OR aub_lbl_ins_code = @barcode
                 OR acb_lbl_ins_code = @barcode
                 OR ocb_lbl_ins_code = @barcode)
            LIMIT 1";

        public string GetItemCodeByBarcode(string barcode)
        {
            var p = new MySqlParameter("@barcode", barcode);
            return GetItemString(GetItemCodeSql, p);
        }

        // ── Scrapped check ────────────────────────────────────────────────────

        private const string CheckScrappedSql = @"
            SELECT COUNT(*) FROM ppt_lbl_instructn_plns_hst
            WHERE  item_code = @itemCode
              AND  lot_no    = @lotNo
              AND  {0}       = 4";   // status column is substituted by caller

        public bool CheckIsScrapped(string statusColumn, string itemCode, string lotNo)
        {
            // statusColumn comes from a fixed internal constant – no user data
            var sql = string.Format(CheckScrappedSql, statusColumn);
            return CheckExists(sql,
                new MySqlParameter("@itemCode", itemCode),
                new MySqlParameter("@lotNo",    lotNo));
        }

        // ── Paired check ──────────────────────────────────────────────────────

        private const string CheckPairedSql = @"
            SELECT COUNT(*) FROM pre_tpc_products_mst
            WHERE  item_code               = @itemCode
              AND  paired_cb_box_type IS NOT NULL
              AND  paired_cb_box_type != ''";

        public bool CheckIsPaired(string itemCode, string lotNo)
        {
            return CheckExists(CheckPairedSql,
                new MySqlParameter("@itemCode", itemCode));
        }

        // ── Latest-barcode lookups ─────────────────────────────────────────────

        private const string GetUpdatedUbBarcodeSql = @"
            SELECT ub_lbl_ins_code FROM ppt_lbl_instructn_plns_hst
            WHERE  item_code = @itemCode AND lot_no = @lotNo
            ORDER BY history_datetime DESC LIMIT 1";

        public string GetUpdatedUbBarcode(string itemCode, string lotNo)
            => GetItemString(GetUpdatedUbBarcodeSql,
                   new MySqlParameter("@itemCode", itemCode),
                   new MySqlParameter("@lotNo",    lotNo));

        // -- Revision / product lookups --------------------------------------

        private const string GetLatestRevisionSql = @"
            SELECT MASTER_LABEL_REVISION_NUMBER AS masterlabel_revision_number
            FROM   ppt_lbl_instructn_plns_tcl
            WHERE  item_code = @itemCode
              AND  lot_no    = @lotNo
            ORDER BY history_datetime DESC
            LIMIT 1";

        public int GetLatestRevision(string itemCode, string lotNo)
        {
            var value = GetItemString(GetLatestRevisionSql,
                new MySqlParameter("@itemCode", itemCode),
                new MySqlParameter("@lotNo", lotNo));

            if (int.TryParse(value, out int revision) && revision > 0)
                return revision;

            return 1;
        }

        private const string GetProductHistorySql = @"
            SELECT barcode_type+0 AS barcode_type, products.*
            FROM   pre_tpc_products_tcl products
            WHERE  item_code = @itemCode
              AND  masterlabel_revision_number = @revision";

        private const string GetProductMasterSql = @"
            SELECT barcode_type+0 AS barcode_type, products.*
            FROM   pre_tpc_products_mst products
            WHERE  item_code = @itemCode";

        public DataTable GetProductData(string itemCode, int revision)
        {
            var history = GetList(GetProductHistorySql,
                new MySqlParameter("@itemCode", itemCode),
                new MySqlParameter("@revision", revision));

            if (history.Rows.Count > 0)
                return history;

            return GetList(GetProductMasterSql,
                new MySqlParameter("@itemCode", itemCode));
        }

        // -- Print-data snapshot ---------------------------------------------

        private const string GetInstructionSnapshotTemplate = @"
            SELECT
                i.item_code,
                i.lot_no,
                i.expiry_date,
                i.{0} AS ins_code,
                i.{1} AS targetqty,
                (SELECT COUNT(*)
                   FROM {2}
                  WHERE item_code = i.item_code
                    AND lot_no = i.lot_no
                    AND label_status IN ('Original','Additional','Replacement','Returned')
                ) AS total_passed,
                (SELECT COUNT(*)
                   FROM {2}
                  WHERE item_code = i.item_code
                    AND lot_no = i.lot_no
                    AND label_status = 'Failed During Printing'
                ) AS total_failed,
                (SELECT COUNT(*)
                   FROM {2}
                  WHERE item_code = i.item_code
                    AND lot_no = i.lot_no
                    AND label_status IN ('First','Last')
                ) AS total_sample,
                (SELECT IFNULL(MAX(sequence_number), 0)
                   FROM {2}
                  WHERE item_code = i.item_code
                    AND lot_no = i.lot_no
                ) AS last_sequence
            FROM   ppt_lbl_instructn_plns_hst i
            WHERE  i.item_code = @itemCode
              AND  i.lot_no = @lotNo
              AND  i.{0} = @instructionCode
              AND  CONCAT(i.history_datetime, i.item_code, i.lot_no) = (
                   SELECT CONCAT(MAX(history_datetime), item_code, lot_no)
                   FROM   ppt_lbl_instructn_plns_hst
                   WHERE  item_code = i.item_code
                     AND  lot_no = i.lot_no)
            LIMIT 1";

        public DataTable GetInstructionPrintSnapshot(string instructionColumn,
                                                     string targetColumn,
                                                     string printTable,
                                                     string itemCode,
                                                     string lotNo,
                                                     string instructionCode)
        {
            var sql = string.Format(GetInstructionSnapshotTemplate,
                                    instructionColumn,
                                    targetColumn,
                                    printTable);

            return GetList(sql,
                new MySqlParameter("@itemCode", itemCode),
                new MySqlParameter("@lotNo", lotNo),
                new MySqlParameter("@instructionCode", instructionCode));
        }

        public long GetMaxSequenceNumber(string printTable, string itemCode, string lotNo)
        {
            string sql = $@"
                SELECT IFNULL(MAX(sequence_number), 0)
                FROM   {printTable}
                WHERE  item_code = @itemCode
                  AND  lot_no = @lotNo";

            string value = GetItemString(sql,
                new MySqlParameter("@itemCode", itemCode),
                new MySqlParameter("@lotNo", lotNo));

            if (long.TryParse(value, out long maxSequence) && maxSequence > 0)
                return maxSequence;

            return 0;
        }

        // ── Print-record existence per table ──────────────────────────────────

        private const string CheckPrintRecordSql = @"
            SELECT COUNT(*) FROM {0}
            WHERE  item_code = @itemCode AND lot_no = @lotNo";

        /// <summary>
        /// Checks whether print records exist in the given table.
        /// <paramref name="tableName"/> must be a fixed constant (never user data).
        /// </summary>
        public bool CheckPrintRecordExists(string tableName, string itemCode, string lotNo)
        {
            var sql = string.Format(CheckPrintRecordSql, tableName);
            return CheckExists(sql,
                new MySqlParameter("@itemCode", itemCode),
                new MySqlParameter("@lotNo",    lotNo));
        }

        // ── Save print results ────────────────────────────────────────────────

        private const string SaveResultSql = @"
            INSERT INTO {0}
                (item_code, lot_no, sequence, printer_name, batch_count, batch_set,
                 label_status, analyzer_result, visual_result, verifier_result,
                 created_user, created_section, ip_address,
                 lastupdate_user, lastupdate_section, lastupdate_ip,
                 paired_type, approved_user, approved_section, approved_ip,
                 created_datetime)
            VALUES
                (@itemCode, @lotNo, @sequence, @printer, @batchCount, @batchSet,
                 @labelStatus, @analyzerResult, @visualResult, @verifierResult,
                 @createdUser, @createdSection, @ipAddress,
                 @lastUpdateUser, @lastUpdateSection, @lastUpdateIp,
                 @pairedType, @approvedUser, @approvedSection, @approvedIp,
                 NOW())";

        /// <summary>
        /// Persists a single printed-label row.
        /// <paramref name="printTable"/> is a fixed constant.
        /// </summary>
        public int SavePrintRecord(string printTable, DataRow row,
                                   string approvedUser, int approvedSection,
                                   string ipAddress)
        {
            var sql = string.Format(SaveResultSql, printTable);
            using var conn = OpenConnection();
            var parameters = new
            {
                itemCode = row["item_code"],
                lotNo = row["lot_no"],
                sequence = row["sequence"],
                printer = row["printer_name"],
                batchCount = row["batch_count"],
                batchSet = row["batch_set"],
                labelStatus = row["label_status"],
                analyzerResult = row["analyzer_result"],
                visualResult = row["visual_result"],
                verifierResult = row["verifier_result"],
                createdUser = row["created_user"],
                createdSection = row["created_section"],
                ipAddress,
                lastUpdateUser = row["created_user"],
                lastUpdateSection = row["created_section"],
                lastUpdateIp = ipAddress,
                pairedType = row["paired_type"],
                approvedUser,
                approvedSection,
                approvedIp = ipAddress,
            };

            return conn.Execute(sql, parameters);
        }

        // ── Table name constants (mirrors VB Define) ──────────────────────────
        public static class Tables
        {
            public const string UbPrint        = "prdprnt_ub_labels_tcl";
            public const string UbPrintHistory = "prdprnt_ub_labels_hst";
            public const string CbPrint        = "prdprnt_cb_labels_tcl";
            public const string CbPrintHistory = "prdprnt_cb_labels_hst";
            public const string OubPrint       = "prdprnt_oub_labels_tcl";
            public const string AubPrint       = "prdprnt_aub_labels_tcl";
            public const string AcbPrint       = "prdprnt_acb_labels_tcl";
            public const string OcbPrint       = "prdprnt_ocb_labels_tcl";
            public const string CasePrint      = "prdprnt_case_labels_tcl";
        }

        // ── Status column constants ───────────────────────────────────────────
        public static class StatusColumns
        {
            public const string UbLiStatus  = "ub_lbl_ins_status";
            public const string CbLiStatus  = "cb_lbl_ins_status";
            public const string OubLiStatus = "oub_lbl_ins_status";
            public const string AubLiStatus = "aub_lbl_ins_status";
            public const string AcbLiStatus = "acb_lbl_ins_status";
            public const string OcbLiStatus = "ocb_lbl_ins_status";
        }
    }
}
