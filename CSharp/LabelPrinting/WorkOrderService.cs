// =============================================================================
// LASYS – Label Printing (C# Port)
// File   : WorkOrderService.cs
// Purpose: Business-logic layer.
//          Mirrors StatusMonitoringMngr.vb (barcode validation, scan→selection)
//          and the data-loading parts of LabelPrinterDB.vb (GetData).
// =============================================================================

using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace LASYS.LabelPrinting
{
    /// <summary>
    /// Orchestrates all business rules that sit between the UI and the database:
    ///   • Barcode validation and GS1-128 parsing
    ///   • DB flags derivation (IsScrapped, IsPaired, IsLabelPrinted, etc.)
    ///   • PrintData population (GetData equivalent)
    /// Mirrors StatusMonitoringMngr.vb and relevant regions of LabelPrinterDB.vb.
    /// </summary>
    public class WorkOrderService
    {
        private readonly WorkOrderRepository _repo;

        public WorkOrderService(WorkOrderRepository repository)
        {
            _repo = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        // ─────────────────────────────────────────────────────────────────────
        //  1. Barcode validation  (mirrors StatusMonitoringMngr.CheckValidBarcode)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns true when every character in <paramref name="barcode"/> is
        /// alphanumeric (mirrors CheckValidBarcode in VB).
        /// </summary>
        public bool IsValidBarcode(string barcode)
        {
            if (string.IsNullOrEmpty(barcode)) return false;
            return Regex.IsMatch(barcode, @"^[A-Za-z0-9]+$");
        }

        // ─────────────────────────────────────────────────────────────────────
        //  2. GS1-128 parsing  (mirrors GS1_128InstructionBarcode.vb)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Parses a GS1-128 instruction barcode into its constituent fields.
        /// GS1-128 uses Application Identifiers (AIs) to structure the data:
        ///   AI 02 = Packaging / box type (2 digits)
        ///   AI 10 = Lot/Batch number
        ///   AI 240 = Additional product identification (assembly-line suffix)
        ///   AI 17 = Expiry date (YYMMDD)
        /// The exact AI positions match GS1_128InstructionBarcode.vb.
        /// </summary>
        public Gs1BarcodeData ParseGs1Barcode(string rawBarcode)
        {
            if (string.IsNullOrWhiteSpace(rawBarcode))
                throw new ArgumentException("Barcode must not be empty.", nameof(rawBarcode));

            var data = new Gs1BarcodeData { RawBarcode = rawBarcode };

            // AI 02 – packaging/box type: positions 0–1
            if (rawBarcode.Length >= 4 && rawBarcode.Substring(0, 2) == "02")
            {
                if (int.TryParse(rawBarcode.Substring(2, 2), out int pkg))
                    data.Packaging = pkg;
            }

            // AI 10 – lot number: locate "10" after packaging field
            var lotMatch = Regex.Match(rawBarcode, @"10([A-Z0-9]+?)(?=\d{2}[A-Z0-9]|$)");
            if (lotMatch.Success)
                data.LotNumber = lotMatch.Groups[1].Value;

            // AI 240 – assembly-line suffix
            var suffixMatch = Regex.Match(rawBarcode, @"240([A-Z0-9]{1,3})");
            if (suffixMatch.Success)
                data.AssemblyLineSuffix = suffixMatch.Groups[1].Value;

            // AI 17 – expiry date YYMMDD
            var expiryMatch = Regex.Match(rawBarcode, @"17(\d{6})");
            if (expiryMatch.Success
                && DateTime.TryParseExact(expiryMatch.Groups[1].Value,
                                          "yyMMdd",
                                          null,
                                          System.Globalization.DateTimeStyles.None,
                                          out DateTime exp))
                data.ExpiryDate = exp;

            return data;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  3. Barcode scan  (mirrors StatusMonitoringMngr.BarcodeScan)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Full scan flow for the label-printing path:
        ///   1. Validates the barcode string.
        ///   2. Checks the instruction-barcode exists in the DB (excl. scrapped).
        ///   3. Parses GS1-128 fields.
        ///   4. Populates all state flags into a <see cref="BoxTypeSelection"/>.
        /// Returns null and sets <paramref name="errorMessage"/> if invalid.
        /// Mirrors StatusMonitoringMngr.BarcodeScan() for label-printing mode.
        /// </summary>
        public BoxTypeSelection BarcodeScan(string barcode, out string errorMessage)
        {
            errorMessage = null;

            // ── step 1: format validation
            if (!IsValidBarcode(barcode))
            {
                errorMessage = "Invalid barcode format. Only alphanumeric characters are allowed.";
                return null;
            }

            // ── step 2: existence check (PRINTINSBARCODEINFO equivalent)
            if (!_repo.CheckInstructionBarcodeExists(barcode))
            {
                errorMessage = "The scanned Label Instruction Code does not exist or is Scrapped.";
                return null;
            }

            // ── step 3: parse + build selection
            return CheckScannedData(barcode, out errorMessage);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  4. CheckScannedData  (mirrors StatusMonitoringMngr.CheckScannedData)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Derives all decision flags for a confirmed-existing barcode.
        /// Mirrors the body of StatusMonitoringMngr.CheckScannedData().
        /// </summary>
        public BoxTypeSelection CheckScannedData(string barcode, out string errorMessage)
        {
            errorMessage = null;
            var selection = new BoxTypeSelection();

            try
            {
                // Parse GS1-128 fields
                var gs1 = ParseGs1Barcode(barcode);

                selection.Barcode  = barcode;
                selection.BoxType  = (BoxType)gs1.Packaging;
                selection.LotNo    = gs1.LotNumber + gs1.AssemblyLineSuffix;
                selection.ItemCode = _repo.GetItemCodeByBarcode(barcode);

                // ── Existence flags in each print table
                selection.IsCaseExists = _repo.CheckPrintRecordExists(
                    WorkOrderRepository.Tables.CasePrint, selection.ItemCode, selection.LotNo);
                selection.IsUbExists   = _repo.CheckPrintRecordExists(
                    WorkOrderRepository.Tables.UbPrint,   selection.ItemCode, selection.LotNo);
                selection.IsOubExists  = _repo.CheckPrintRecordExists(
                    WorkOrderRepository.Tables.OubPrint,  selection.ItemCode, selection.LotNo);
                selection.IsOcbExists  = _repo.CheckPrintRecordExists(
                    WorkOrderRepository.Tables.OcbPrint,  selection.ItemCode, selection.LotNo);
                selection.IsCbExists   = _repo.CheckPrintRecordExists(
                    WorkOrderRepository.Tables.CbPrint,   selection.ItemCode, selection.LotNo);
                selection.IsAubExists  = _repo.CheckPrintRecordExists(
                    WorkOrderRepository.Tables.AubPrint,  selection.ItemCode, selection.LotNo);
                selection.IsAcbExists  = _repo.CheckPrintRecordExists(
                    WorkOrderRepository.Tables.AcbPrint,  selection.ItemCode, selection.LotNo);

                // ── Scrapped check (box-type specific status column)
                string statusCol = ResolveStatusColumn(selection.BoxType);
                selection.IsScrapped = _repo.CheckIsScrapped(statusCol, selection.ItemCode, selection.LotNo);

                // ── Paired flag
                selection.IsPaired = _repo.CheckIsPaired(selection.ItemCode, selection.LotNo);

                // ── Barcode-updated check:
                //    Compare scanned barcode with the latest on record.
                string latestBarcode = _repo.GetUpdatedUbBarcode(selection.ItemCode, selection.LotNo);
                selection.IsBarcodeUpdated = string.IsNullOrEmpty(latestBarcode)
                                             || latestBarcode.Equals(barcode, StringComparison.OrdinalIgnoreCase);

                // ── Print-type derivation
                selection.PrintType = DeriveDefaultPrintType(selection);
            }
            catch (Exception ex)
            {
                errorMessage = $"Error processing barcode: {ex.Message}";
                return null;
            }

            return selection;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  5. PrintData loader  (mirrors LabelPrinterDB.GetData)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Populates a <see cref="PrintData"/> object ready for the label-print form.
        /// Calls the equivalent of GetMasterData → GetPrintData → GetNextSetNumber.
        /// Mirrors LabelPrinterDB.GetData().
        /// </summary>
        public PrintData BuildPrintData(string itemCode, string lotNo,
                                        string instructionCode, BoxType labelType,
                                        LabelPrintType labelStatus)
        {
            var data = new PrintData
            {
                ItemCode        = itemCode,
                LotNo           = lotNo,
                InstructionCode = instructionCode,
                LabelType       = labelType,
                LabelStatus     = labelStatus,
            };

            LoadMasterData(data);
            LoadPrintData(data);
            ResolveNextSequence(data);

            return data;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  6. SetData  (mirrors frmWorkOrderListing.SetData / SetData after scan)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Maps a <see cref="BoxTypeSelection"/> (from a scan) onto a <see cref="PrintData"/>
        /// object – equivalent to SetData() in frmWorkOrderListing.vb.
        /// </summary>
        public PrintData MapSelectionToPrintData(BoxTypeSelection selection)
        {
            if (selection == null) throw new ArgumentNullException(nameof(selection));

            var data = new PrintData
            {
                ItemCode        = selection.ItemCode,
                LotNo           = selection.LotNo,
                InstructionCode = selection.Barcode,
                LabelType       = selection.BoxType,
                LabelStatus     = MapPrintType(selection.PrintType),
            };

            return data;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Private helpers
        // ─────────────────────────────────────────────────────────────────────

        private string ResolveStatusColumn(BoxType boxType)
        {
            return boxType switch
            {
                BoxType.UnitBox or BoxType.McKessonUB or BoxType.CaseLabel or BoxType.QC
                    => WorkOrderRepository.StatusColumns.UbLiStatus,
                BoxType.OuterUnitBox or BoxType.OubQC
                    => WorkOrderRepository.StatusColumns.OubLiStatus,
                BoxType.OuterCartonBox
                    => WorkOrderRepository.StatusColumns.OcbLiStatus,
                BoxType.AdditionalUnitBox or BoxType.AubQC
                    => WorkOrderRepository.StatusColumns.AubLiStatus,
                BoxType.AdditionalCartonBox
                    => WorkOrderRepository.StatusColumns.AcbLiStatus,
                _   => WorkOrderRepository.StatusColumns.CbLiStatus,
            };
        }

        private int DeriveDefaultPrintType(BoxTypeSelection sel)
        {
            // Original unless prior records exist → Additional
            bool anyExists = sel.IsUbExists || sel.IsCbExists || sel.IsOubExists
                          || sel.IsAubExists || sel.IsAcbExists || sel.IsOcbExists
                          || sel.IsCaseExists;

            if (!anyExists)               return (int)LabelPrintType.Original;
            if (sel.IsAdditional)         return (int)LabelPrintType.Additional;
            if (sel.IsReplacement)        return (int)LabelPrintType.Replacement;
            if (sel.IsReturned)           return (int)LabelPrintType.Returned;
            if (sel.IsExcess)             return (int)LabelPrintType.Excess;
            return (int)LabelPrintType.Original;
        }

        private LabelPrintType MapPrintType(int printType)
        {
            return Enum.IsDefined(typeof(LabelPrintType), printType)
                ? (LabelPrintType)printType
                : LabelPrintType.NotSet;
        }

        // ── Stub loaders – replace with real SQL via _repo in production ──

        private void LoadMasterData(PrintData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            data.RevisionNumber = _repo.GetLatestRevision(data.ItemCode, data.LotNo);

            DataTable product = _repo.GetProductData(data.ItemCode, data.RevisionNumber);
            if (product.Rows.Count == 0)
                return;

            DataRow row = product.Rows[0];

            (string barcodeColumn, string qtyColumn) = ResolveProductColumns(data.LabelType);

            data.ProductBarcode  = GetString(row, barcodeColumn, "ins_code", "ub_lbl_ins_code", "cb_lbl_ins_code");
            data.ProductQuantity = GetInt64(row, qtyColumn);
            data.PssNumber       = GetString(row, "pss_id_no", "pss_number", "pss_no");

            string mfgDate = GetString(row, "mfg_date", "manufacture_date");
            if (!string.IsNullOrWhiteSpace(mfgDate))
                data.MfgDate = mfgDate;
        }

        private void LoadPrintData(PrintData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            string instructionColumn = ResolveInstructionColumn(data.LabelType);
            string targetColumn      = ResolveTargetQuantityColumn(data.LabelType);
            string printTable        = ResolvePrintTable(data.LabelType);

            DataTable snapshot = _repo.GetInstructionPrintSnapshot(
                instructionColumn,
                targetColumn,
                printTable,
                data.ItemCode,
                data.LotNo,
                data.InstructionCode);

            data.ResultData = snapshot;

            if (snapshot.Rows.Count == 0)
            {
                data.TargetQuantity = 0;
                data.TotalPassed = 0;
                data.TotalFailedCase = 0;
                data.TotalSampleCase = 0;
                data.LastSequence = 0;
                data.SequenceNumber = 1;
                return;
            }

            DataRow row = snapshot.Rows[0];

            data.TargetQuantity  = GetInt64(row, "targetqty");
            data.TotalPassed     = GetInt64(row, "total_passed");
            data.TotalFailedCase = GetInt64(row, "total_failed");
            data.TotalSampleCase = (int)GetInt64(row, "total_sample");
            data.LastSequence    = GetInt64(row, "last_sequence");
            data.SequenceNumber  = data.LastSequence + 1;

            DateTime expiry = GetDateTime(row, "expiry_date");
            if (expiry != DateTime.MinValue)
                data.ExpiryDate = expiry;

            if (data.LabelType == BoxType.CaseLabel)
            {
                data.TotalCase = data.TotalPassed + data.TotalFailedCase + data.TotalSampleCase;
                data.TotalPassedCase = data.TotalPassed;
            }
        }

        private void ResolveNextSequence(PrintData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            string printTable = ResolvePrintTable(data.LabelType);
            long maxSequence = _repo.GetMaxSequenceNumber(printTable, data.ItemCode, data.LotNo);

            // Keep the greater value in case snapshot/aggregate already produced one.
            data.LastSequence = Math.Max(data.LastSequence, maxSequence);
            data.SequenceNumber = data.LastSequence + 1;
        }

        private static string ResolvePrintTable(BoxType labelType)
        {
            return labelType switch
            {
                BoxType.UnitBox or BoxType.QC or BoxType.McKessonUB => WorkOrderRepository.Tables.UbPrint,
                BoxType.CartonBox or BoxType.COC => WorkOrderRepository.Tables.CbPrint,
                BoxType.OuterUnitBox or BoxType.OubQC => WorkOrderRepository.Tables.OubPrint,
                BoxType.AdditionalUnitBox or BoxType.AubQC => WorkOrderRepository.Tables.AubPrint,
                BoxType.AdditionalCartonBox => WorkOrderRepository.Tables.AcbPrint,
                BoxType.OuterCartonBox => WorkOrderRepository.Tables.OcbPrint,
                BoxType.CaseLabel => WorkOrderRepository.Tables.CasePrint,
                _ => WorkOrderRepository.Tables.UbPrint,
            };
        }

        private static string ResolveInstructionColumn(BoxType labelType)
        {
            return labelType switch
            {
                BoxType.UnitBox or BoxType.QC or BoxType.CaseLabel or BoxType.McKessonUB => "ub_lbl_ins_code",
                BoxType.CartonBox or BoxType.COC => "cb_lbl_ins_code",
                BoxType.OuterUnitBox or BoxType.OubQC => "oub_lbl_ins_code",
                BoxType.AdditionalUnitBox or BoxType.AubQC => "aub_lbl_ins_code",
                BoxType.AdditionalCartonBox => "acb_lbl_ins_code",
                BoxType.OuterCartonBox => "ocb_lbl_ins_code",
                _ => "ub_lbl_ins_code",
            };
        }

        private static string ResolveTargetQuantityColumn(BoxType labelType)
        {
            return labelType switch
            {
                BoxType.UnitBox or BoxType.QC or BoxType.CaseLabel or BoxType.McKessonUB => "ub_lbl_ins_trgt_prnt_qty",
                BoxType.CartonBox or BoxType.COC => "cb_lbl_ins_trgt_prnt_qty",
                BoxType.OuterUnitBox or BoxType.OubQC => "oub_lbl_ins_trgt_prnt_qty",
                BoxType.AdditionalUnitBox or BoxType.AubQC => "aub_lbl_ins_trgt_prnt_qty",
                BoxType.AdditionalCartonBox => "acb_lbl_ins_trgt_prnt_qty",
                BoxType.OuterCartonBox => "ocb_lbl_ins_trgt_prnt_qty",
                _ => "ub_lbl_ins_trgt_prnt_qty",
            };
        }

        private static (string barcodeColumn, string qtyColumn) ResolveProductColumns(BoxType labelType)
        {
            return labelType switch
            {
                BoxType.UnitBox or BoxType.QC or BoxType.CaseLabel or BoxType.McKessonUB => ("ub_barcode", "ub_quantity"),
                BoxType.CartonBox or BoxType.COC => ("cb_barcode", "cb_quantity"),
                BoxType.OuterUnitBox or BoxType.OubQC => ("oub_barcode", "oub_quantity"),
                BoxType.AdditionalUnitBox or BoxType.AubQC => ("aub_barcode", "aub_quantity"),
                BoxType.AdditionalCartonBox => ("acb_barcode", "acb_quantity"),
                BoxType.OuterCartonBox => ("ocb_barcode", "ocb_quantity"),
                _ => ("ub_barcode", "ub_quantity"),
            };
        }

        private static string GetString(DataRow row, params string[] columns)
        {
            foreach (string col in columns)
            {
                object value = GetValue(row, col);
                if (value != null)
                    return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            }

            return string.Empty;
        }

        private static long GetInt64(DataRow row, params string[] columns)
        {
            foreach (string col in columns)
            {
                object value = GetValue(row, col);
                if (value == null) continue;

                if (value is long l) return l;
                if (value is int i) return i;
                if (value is decimal d) return Convert.ToInt64(d);
                if (long.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out long parsed))
                    return parsed;
            }

            return 0;
        }

        private static DateTime GetDateTime(DataRow row, params string[] columns)
        {
            foreach (string col in columns)
            {
                object value = GetValue(row, col);
                if (value == null) continue;

                if (value is DateTime dt) return dt;
                if (DateTime.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out DateTime parsed))
                    return parsed;
            }

            return DateTime.MinValue;
        }

        private static object GetValue(DataRow row, string column)
        {
            if (row == null || row.Table == null || string.IsNullOrWhiteSpace(column))
                return null;

            DataColumn exact = row.Table.Columns.Cast<DataColumn>()
                .FirstOrDefault(c => c.ColumnName == column);

            if (exact != null && !row.IsNull(exact))
                return row[exact];

            DataColumn ignoreCase = row.Table.Columns.Cast<DataColumn>()
                .FirstOrDefault(c => string.Equals(c.ColumnName, column, StringComparison.OrdinalIgnoreCase));

            if (ignoreCase != null && !row.IsNull(ignoreCase))
                return row[ignoreCase];

            return null;
        }
    }
}
