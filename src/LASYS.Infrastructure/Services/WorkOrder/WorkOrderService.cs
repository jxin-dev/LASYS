using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Models;
using LASYS.Application.Interfaces.Persistence.Repositories;
using LASYS.Domain.Product;
using LASYS.Infrastructure.Persistence.Repositories;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace LASYS.Infrastructure.Services.WorkOrder
{
    public class WorkOrderService
    {
        private readonly IWorkOrderRepository _repo;

        public WorkOrderService(IWorkOrderRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Returns true when every character in <paramref name="barcode"/> is
        /// alphanumeric (mirrors CheckValidBarcode in VB).
        /// </summary>
        public bool IsValidBarcode(string barcode)
        {
            if (string.IsNullOrEmpty(barcode)) return false;
            return Regex.IsMatch(barcode, @"^[A-Za-z0-9]+$");
        }

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

        /// <summary>
        /// Full scan flow for the label-printing path:
        ///   1. Validates the barcode string.
        ///   2. Checks the instruction-barcode exists in the DB (excl. scrapped).
        ///   3. Parses GS1-128 fields.
        ///   4. Populates all state flags into a <see cref="BoxTypeSelection"/>.
        /// Returns null and sets <paramref name="errorMessage"/> if invalid.
        /// Mirrors StatusMonitoringMngr.BarcodeScan() for label-printing mode.
        /// </summary>
        public async Task<(BoxTypeSelection? Selection, string ErrorMessage)> BarcodeScan(string barcode)
        {
            string errorMessage = string.Empty;

            // ── step 1: format validation
            if (!IsValidBarcode(barcode))
            {
                errorMessage = "Invalid barcode format. Only alphanumeric characters are allowed.";
                return (null, errorMessage);
            }

            // ── step 2: existence check (PRINTINSBARCODEINFO equivalent)
            if (!await _repo.CheckInstructionBarcodeExistsAsync(barcode))
            {
                errorMessage = "The scanned Label Instruction Code does not exist or is Scrapped.";
                return (null, errorMessage);
            }

            // ── step 3: parse + build selection
            return await CheckScannedData(barcode);
        }

        // Change the method signature to remove the 'out' parameter and return a tuple instead.
        // Update all usages of this method accordingly.

        public async Task<(BoxTypeSelection? Selection, string ErrorMessage)> CheckScannedData(string barcode)
        {
            string errorMessage = string.Empty;
            var selection = new BoxTypeSelection();

            try
            {
                // Parse GS1-128 fields
                var gs1 = ParseGs1Barcode(barcode);

                selection.Barcode = barcode;
                selection.BoxType = (BoxType)gs1.Packaging;
                selection.LotNo = gs1.LotNumber + gs1.AssemblyLineSuffix;
                selection.ItemCode = await _repo.GetItemCodeByBarcodeAsync(barcode);

                // ── Existence flags in each print table
                selection.IsCaseExists = await _repo.CheckPrintRecordExistsAsync(
                    WorkOrderRepository.Tables.CasePrint, selection.ItemCode, selection.LotNo);
                selection.IsUbExists = await _repo.CheckPrintRecordExistsAsync(
                    WorkOrderRepository.Tables.UbPrint, selection.ItemCode, selection.LotNo);
                selection.IsOubExists = await _repo.CheckPrintRecordExistsAsync(
                    WorkOrderRepository.Tables.OubPrint, selection.ItemCode, selection.LotNo);
                selection.IsOcbExists = await _repo.CheckPrintRecordExistsAsync(
                    WorkOrderRepository.Tables.OcbPrint, selection.ItemCode, selection.LotNo);
                selection.IsCbExists = await _repo.CheckPrintRecordExistsAsync(
                    WorkOrderRepository.Tables.CbPrint, selection.ItemCode, selection.LotNo);
                selection.IsAubExists = await _repo.CheckPrintRecordExistsAsync(
                    WorkOrderRepository.Tables.AubPrint, selection.ItemCode, selection.LotNo);
                selection.IsAcbExists = await _repo.CheckPrintRecordExistsAsync(
                    WorkOrderRepository.Tables.AcbPrint, selection.ItemCode, selection.LotNo);

                // ── Scrapped check (box-type specific status column)
                string statusCol = ResolveStatusColumn(selection.BoxType);
                selection.IsScrapped = await _repo.CheckIsScrappedAsync(statusCol, selection.ItemCode, selection.LotNo);

                // ── Paired flag
                selection.IsPaired = await _repo.CheckIsPairedAsync(selection.ItemCode, selection.LotNo);

                // ── Barcode-updated check:
                //    Compare scanned barcode with the latest on record.
                string latestBarcode = await _repo.GetUpdatedUbBarcodeAsync(selection.ItemCode, selection.LotNo);
                selection.IsBarcodeUpdated = string.IsNullOrEmpty(latestBarcode)
                                             || latestBarcode.Equals(barcode, StringComparison.OrdinalIgnoreCase);

                // ── Print-type derivation
                selection.PrintType = DeriveDefaultPrintType(selection);
            }
            catch (Exception ex)
            {
                errorMessage = $"Error processing barcode: {ex.Message}";
                return (null, errorMessage);
            }

            return (selection, errorMessage);
        }

        /// <summary>
        /// Populates a <see cref="PrintData"/> object ready for the label-print form.
        /// Calls the equivalent of GetMasterData → GetPrintData → GetNextSetNumber.
        /// Mirrors LabelPrinterDB.GetData().
        /// </summary>
        public async Task<PrintData> BuildPrintDataAsync(string itemCode, string lotNo,
                                        string instructionCode, BoxType labelType,
                                        LabelPrintType labelStatus)
        {
            var data = new PrintData
            {
                ItemCode = itemCode,
                LotNo = lotNo,
                InstructionCode = instructionCode,
                LabelType = labelType,
                LabelStatus = labelStatus,
            };

            await LoadMasterData(data);
            await LoadPrintData(data);
            await ResolveNextSequence(data);

            return data;
        }

        /// <summary>
        /// Maps a <see cref="BoxTypeSelection"/> (from a scan) onto a <see cref="PrintData"/>
        /// object – equivalent to SetData() in frmWorkOrderListing.vb.
        /// </summary>
        public PrintData MapSelectionToPrintData(BoxTypeSelection selection)
        {
            if (selection == null) throw new ArgumentNullException(nameof(selection));

            var data = new PrintData
            {
                ItemCode = selection.ItemCode,
                LotNo = selection.LotNo,
                InstructionCode = selection.Barcode,
                LabelType = selection.BoxType,
                LabelStatus = MapPrintType(selection.PrintType),
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
                _ => WorkOrderRepository.StatusColumns.CbLiStatus,
            };
        }
        private int DeriveDefaultPrintType(BoxTypeSelection sel)
        {
            // Original unless prior records exist → Additional
            bool anyExists = sel.IsUbExists || sel.IsCbExists || sel.IsOubExists
                          || sel.IsAubExists || sel.IsAcbExists || sel.IsOcbExists
                          || sel.IsCaseExists;

            if (!anyExists) return (int)LabelPrintType.Original;
            if (sel.IsAdditional) return (int)LabelPrintType.Additional;
            if (sel.IsReplacement) return (int)LabelPrintType.Replacement;
            if (sel.IsReturned) return (int)LabelPrintType.Returned;
            if (sel.IsExcess) return (int)LabelPrintType.Excess;
            return (int)LabelPrintType.Original;
        }

        private LabelPrintType MapPrintType(int printType)
        {
            return Enum.IsDefined(typeof(LabelPrintType), printType)
                ? (LabelPrintType)printType
                : LabelPrintType.NotSet;
        }

        private async Task LoadMasterData(PrintData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            data.RevisionNumber = await _repo.GetLatestRevisionAsync(data.ItemCode, data.LotNo);

            var product = await _repo.GetProductDataAsync(data.ItemCode, data.RevisionNumber);
            if (!product.Any())
                return;

            ProductMaster row = product.First();

            (string barcodeColumn, string qtyColumn) = ResolveProductColumns(data.LabelType);

            // Use ProductMaster-aware helpers (row is not a DataRow)
            data.ProductBarcode = GetProductString(row, barcodeColumn, "ins_code", "ub_lbl_ins_code", "cb_lbl_ins_code");
            data.ProductQuantity = GetProductInt64(row, qtyColumn);
            data.PssNumber = GetProductString(row, "pss_id_no", "pss_number", "pss_no");

            string mfgDate = GetProductString(row, "mfg_date", "manufacture_date");
            if (!string.IsNullOrWhiteSpace(mfgDate))
                data.MfgDate = mfgDate;
        }

        private async Task LoadPrintData(PrintData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            string instructionColumn = ResolveInstructionColumn(data.LabelType);
            string targetColumn = ResolveTargetQuantityColumn(data.LabelType);
            string printTable = ResolvePrintTable(data.LabelType);

            var snapshot = await _repo.GetInstructionPrintSnapshotAsync(
                instructionColumn,
                targetColumn,
                printTable,
                data.ItemCode,
                data.LotNo,
                data.InstructionCode);

            data.ResultData = snapshot;

            if (snapshot == null)
            {
                data.TargetQuantity = 0;
                data.TotalPassed = 0;
                data.TotalFailedCase = 0;
                data.TotalSampleCase = 0;
                data.LastSequence = 0;
                data.SequenceNumber = 1;
                return;
            }

            data.TargetQuantity = snapshot.TargetQty ?? 0;
            data.TotalPassed = snapshot.TotalPassed; //GetInt64(row, "total_passed");
            data.TotalFailedCase = snapshot.TotalFailed;
            data.TotalSampleCase = (int)snapshot.TotalSample;
            data.LastSequence = snapshot.LastSequence;
            data.SequenceNumber = data.LastSequence + 1;

            if (snapshot.ExpiryDate != null && snapshot.ExpiryDate != DateTime.MinValue)
                data.ExpiryDate = snapshot.ExpiryDate.Value;

            if (data.LabelType == BoxType.CaseLabel)
            {
                data.TotalCase = data.TotalPassed + data.TotalFailedCase + data.TotalSampleCase;
                data.TotalPassedCase = data.TotalPassed;
            }
        }
        private async Task ResolveNextSequence(PrintData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            string printTable = ResolvePrintTable(data.LabelType);
            long maxSequence = await _repo.GetMaxSequenceNumberAsync(printTable, data.ItemCode, data.LotNo);

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

        // Helpers to read values from ProductMaster (used when GetProductDataAsync returns domain objects)
        private static object? GetProductValue(ProductMaster row, string column)
        {
            if (row == null || string.IsNullOrWhiteSpace(column))
                return null;

            string key = column.Replace("_", string.Empty).ToLowerInvariant();

            // common barcode / number shortcuts
            switch (key)
            {
                case "ubbarcode":
                    if (!string.IsNullOrWhiteSpace(row.UbBarcode)) return row.UbBarcode;
                    if (!string.IsNullOrWhiteSpace(row.UbBarcodeNumber)) return row.UbBarcodeNumber;
                    break;
                case "cbbarcode":
                    if (!string.IsNullOrWhiteSpace(row.CbBarcode)) return row.CbBarcode;
                    if (!string.IsNullOrWhiteSpace(row.CbBarcodeNumber)) return row.CbBarcodeNumber;
                    break;
                case "oubbarcode":
                    if (!string.IsNullOrWhiteSpace(row.OubBarcode)) return row.OubBarcode;
                    if (!string.IsNullOrWhiteSpace(row.OubBarcodeNumber)) return row.OubBarcodeNumber;
                    break;
                case "aubbarcode":
                    if (!string.IsNullOrWhiteSpace(row.AubBarcode)) return row.AubBarcode;
                    if (!string.IsNullOrWhiteSpace(row.AubBarcodeNumber)) return row.AubBarcodeNumber;
                    break;
                case "acbbarcode":
                    if (!string.IsNullOrWhiteSpace(row.AcbBarcode)) return row.AcbBarcode;
                    if (!string.IsNullOrWhiteSpace(row.AcbBarcodeNumber)) return row.AcbBarcodeNumber;
                    break;
                case "ocbbarcode":
                    if (!string.IsNullOrWhiteSpace(row.OcbBarcode)) return row.OcbBarcode;
                    if (!string.IsNullOrWhiteSpace(row.OcbBarcodeNumber)) return row.OcbBarcodeNumber;
                    break;
            }

            // Reflection fallback: match property by normalizing name (remove underscores, case-insensitive)
            var props = typeof(ProductMaster).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var p in props)
            {
                var normalized = p.Name.Replace("_", string.Empty).ToLowerInvariant();
                if (normalized == key)
                {
                    var val = p.GetValue(row);
                    if (val != null) return val;
                    break;
                }
            }

            // Try a case-insensitive property name match as last resort
            var propByName = props.FirstOrDefault(p => string.Equals(p.Name, column, StringComparison.OrdinalIgnoreCase));
            if (propByName != null)
            {
                var val = propByName.GetValue(row);
                if (val != null) return val;
            }

            return null;
        }

        private static string GetProductString(ProductMaster row, params string[] columns)
        {
            foreach (string col in columns)
            {
                object? value = GetProductValue(row, col);
                if (value != null)
                    return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            }

            return string.Empty;
        }

        private static long GetProductInt64(ProductMaster row, params string[] columns)
        {
            foreach (string col in columns)
            {
                object? value = GetProductValue(row, col);
                if (value == null) continue;

                if (value is long l) return l;
                if (value is int i) return i;
                if (value is decimal d) return Convert.ToInt64(d);
                if (long.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out long parsed))
                    return parsed;
            }

            return 0;
        }

        private static string GetString(DataRow row, params string[] columns)
        {
            foreach (string col in columns)
            {
                object? value = GetValue(row, col);
                if (value != null)
                    return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            }

            return string.Empty;
        }

        private static long GetInt64(DataRow row, params string[] columns)
        {
            foreach (string col in columns)
            {
                object? value = GetValue(row, col);
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
                object? value = GetValue(row, col);
                if (value == null) continue;

                if (value is DateTime dt) return dt;
                if (DateTime.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out DateTime parsed))
                    return parsed;
            }

            return DateTime.MinValue;
        }

        private static object? GetValue(DataRow row, string column)
        {
            if (row == null || row.Table == null || string.IsNullOrWhiteSpace(column))
                return null;

            DataColumn? exact = row.Table.Columns.Cast<DataColumn>()
                .FirstOrDefault(c => c.ColumnName == column);

            if (exact != null && !row.IsNull(exact))
                return row[exact];

            DataColumn? ignoreCase = row.Table.Columns.Cast<DataColumn>()
                .FirstOrDefault(c => string.Equals(c.ColumnName, column, StringComparison.OrdinalIgnoreCase));

            if (ignoreCase != null && !row.IsNull(ignoreCase))
                return row[ignoreCase];

            return null;
        }
    }
}
