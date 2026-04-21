// =============================================================================
// LASYS – Label Printing (C# Port)
// File   : Forms\LabelPrintForm.cs
// Purpose: Label-print execution form.
//          Mirrors frmLabelPrint.vb – loads print settings, executes the
//          SATO printer command, triggers the analyzer, and saves results.
// =============================================================================

using System;
using System.Data;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace LASYS.LabelPrinting.Forms
{
    /// <summary>
    /// Label-print execution form.
    /// Mirrors frmLabelPrint.vb (LAS31).
    ///
    /// Flow once the form opens:
    ///   1. <see cref="LabelPrintForm_Load"/> populates UI fields from PrintData.
    ///   2. Operator reviews / adjusts quantity and clicks Print.
    ///   3. <see cref="BtnPrint_Click"/> → <see cref="ExecutePrint"/> on a
    ///      background thread.
    ///   4. <see cref="ExecutePrint"/> builds the PRN command, sends bytes to
    ///      the SATO printer via Win32 APIs, and collects analyzer results.
    ///   5. <see cref="SaveResults"/> persists the outcome in the DB.
    /// </summary>
    public partial class LabelPrintForm : Form
    {
        // ── Win32 print API declarations  (mirrors LabelPrinter.vb) ──────────
        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterW",
                   SetLastError = true, CharSet = CharSet.Unicode,
                   ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool OpenPrinter(string pPrinterName,
                                               ref IntPtr phPrinter,
                                               long pDefault);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter",
                   SetLastError = true, CharSet = CharSet.Unicode,
                   ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterW",
                   SetLastError = true, CharSet = CharSet.Unicode,
                   ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern int StartDocPrinter(IntPtr hPrinter,
                                                  int level,
                                                  ref DocInfo1 pDocInfo);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter",
                   SetLastError = true, CharSet = CharSet.Unicode,
                   ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter",
                   SetLastError = true, CharSet = CharSet.Unicode,
                   ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter",
                   SetLastError = true, CharSet = CharSet.Unicode,
                   ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter",
                   SetLastError = true, CharSet = CharSet.Unicode,
                   ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool WritePrinter(IntPtr hPrinter,
                                                IntPtr pBytes,
                                                int dwCount,
                                                ref int dwWritten);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct DocInfo1
        {
            [MarshalAs(UnmanagedType.LPWStr)] public string pDocName;
            [MarshalAs(UnmanagedType.LPWStr)] public string pOutputFile;
            [MarshalAs(UnmanagedType.LPWStr)] public string pDataType;
        }

        // ── Dependencies & state ──────────────────────────────────────────────
        private readonly WorkOrderRepository _repo;
        private readonly PrintData           _printData;

        private Thread _printThread;
        private volatile bool _isPrinting;
        private volatile bool _isPaused;

        // ── Constants ─────────────────────────────────────────────────────────
        private const string PrintDocumentName = "LASYS Label";
        private const string RawDataType       = "RAW";

        // ─────────────────────────────────────────────────────────────────────
        //  Constructor
        // ─────────────────────────────────────────────────────────────────────

        public LabelPrintForm(WorkOrderRepository repo, PrintData printData)
        {
            _repo      = repo      ?? throw new ArgumentNullException(nameof(repo));
            _printData = printData ?? throw new ArgumentNullException(nameof(printData));

            InitializeComponent();
            WireEvents();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Form load
        // ─────────────────────────────────────────────────────────────────────

        private void LabelPrintForm_Load(object sender, EventArgs e)
        {
            PopulateHeader();
            LoadPrinterList();
            txtQuantity.Text = _printData.TargetQuantity.ToString();
            txtQuantity.Focus();
            txtQuantity.SelectAll();
        }

        private void PopulateHeader()
        {
            lblItemCode.Text  = $"Item Code : {_printData.ItemCode}";
            lblLotNo.Text     = $"Lot No    : {_printData.LotNo}";
            lblLabelType.Text = $"Label Type: {_printData.LabelType}";
            lblStatus.Text    = $"Status    : {_printData.LabelStatus}";
            lblSequence.Text  = $"Sequence  : {_printData.LastSequence + 1}";
        }

        private void LoadPrinterList()
        {
            cboPrinter.Items.Clear();
            foreach (string p in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
                cboPrinter.Items.Add(p);

            // Pre-select the default printer
            var defaultSettings = new System.Drawing.Printing.PrinterSettings();
            if (!string.IsNullOrEmpty(defaultSettings.PrinterName))
                cboPrinter.SelectedItem = defaultSettings.PrinterName;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Button handlers
        // ─────────────────────────────────────────────────────────────────────

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            if (_isPrinting)
            {
                // Toggle Pause / Resume
                _isPaused = !_isPaused;
                btnPrint.Text = _isPaused ? "Resume" : "Pause";
                return;
            }

            if (!ValidatePrintInput()) return;

            _isPrinting   = true;
            btnPrint.Text = "Pause";
            btnStop.Enabled = true;

            _printThread = new Thread(ExecutePrint) { IsBackground = true };
            _printThread.Start();
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            _isPrinting = false;
            _isPaused   = false;
            _printThread?.Interrupt();
            SetPrintButtonState(ready: true);
            AppendLog("Printing stopped by operator.");
        }

        private void BtnClose_Click(object sender, EventArgs e) => Close();

        // ─────────────────────────────────────────────────────────────────────
        //  Input validation  (mirrors ValidateInput in frmLabelPrint.vb)
        // ─────────────────────────────────────────────────────────────────────

        private bool ValidatePrintInput()
        {
            if (cboPrinter.SelectedItem == null)
            {
                MessageBox.Show("Please select a Printer.",
                                "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtQuantity.Text))
            {
                MessageBox.Show("Please input a 'Quantity' value.",
                                "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!long.TryParse(txtQuantity.Text, out long qty) || qty < 1)
            {
                MessageBox.Show("Please input a valid numeric 'Quantity' value (minimum 1).",
                                "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (qty + _printData.TotalPassed > _printData.TargetQuantity)
            {
                MessageBox.Show("Print quantity will exceed the maximum target label for the current lot.",
                                "Quantity Exceeded", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Print execution  (mirrors PrintBarcode thread logic in frmLabelPrint.vb)
        // ─────────────────────────────────────────────────────────────────────

        private void ExecutePrint()
        {
            long   quantity    = 0;
            string printerName = string.Empty;

            Invoke((Action)(() =>
            {
                quantity    = long.Parse(txtQuantity.Text);
                printerName = cboPrinter.SelectedItem?.ToString() ?? string.Empty;
            }));

            try
            {
                for (long seq = _printData.LastSequence + 1;
                     seq <= _printData.LastSequence + quantity && _isPrinting;
                     seq++)
                {
                    // ── Pause handling
                    while (_isPaused && _isPrinting) Thread.Sleep(200);
                    if (!_isPrinting) break;

                    // ── 1. Load the PRN template for this label type
                    byte[] prnData = LoadPrnTemplate(_printData);

                    // ── 2. Substitute job-id / sequence / quantity into command
                    byte[] command = UpdatePrintCommand(prnData, _printData.InstructionCode,
                                                        seq, quantity,
                                                        _printData.LabelType);

                    // ── 3. Send to SATO via Win32 WritePrinter
                    bool printed = SendBytesToPrinter(printerName, command, PrintDocumentName);

                    // ── 4. Build result row
                    var resultRow = BuildResultRow(seq, printerName, printed);

                    // ── 5. Optionally trigger analyzer (if enabled)
                    if (chkAnalyzer.Checked)
                        CollectAnalyzerResult(resultRow, seq);

                    // ── 6. Persist this label's result
                    SaveSingleResult(resultRow);

                    int progress = (int)((seq - _printData.LastSequence) * 100 / quantity);
                    Invoke((Action)(() =>
                    {
                        pgbPrint.Value    = Math.Min(progress, 100);
                        lblSequence.Text = $"Sequence  : {seq}";
                        AppendLog($"Label {seq} – {(printed ? "OK" : "FAILED")}");
                    }));
                }

                Invoke((Action)(() =>
                {
                    MessageBox.Show("Printing completed.", "Done",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                    SetPrintButtonState(ready: true);
                }));
            }
            catch (ThreadInterruptedException)
            {
                // Stopped by operator – normal.
            }
            catch (Exception ex)
            {
                Invoke((Action)(() =>
                {
                    MessageBox.Show($"Barcode label printing failed.\n{ex.Message}",
                                    "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    SetPrintButtonState(ready: true);
                }));
            }
            finally
            {
                _isPrinting = false;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  PRN template loader  (mirrors InstructionBarcode / MInstructionBarcode)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Loads the binary .prn / .lbl file for the given label type.
        /// In the original VB code this relies on LabelGallery/NiceLabel COM
        /// objects to export the label to a byte array.
        /// Here we read the pre-exported PRN from disk (equivalent path logic).
        /// </summary>
        private byte[] LoadPrnTemplate(PrintData pd)
        {
            string labelDir  = GetLabelDirectory(pd.LabelType);
            string labelFile = Path.Combine(labelDir, $"{pd.InstructionCode}.prn");

            if (!File.Exists(labelFile))
                throw new FileNotFoundException(
                    $"Label template not found: {labelFile}", labelFile);

            return File.ReadAllBytes(labelFile);
        }

        private static string GetLabelDirectory(BoxType labelType)
        {
            // Mirrors the LabelFileSettings.xml / XMLSettings path resolution.
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            return labelType switch
            {
                BoxType.UnitBox             => Path.Combine(baseDir, "labels", "UB"),
                BoxType.CartonBox           => Path.Combine(baseDir, "labels", "CB"),
                BoxType.OuterUnitBox        => Path.Combine(baseDir, "labels", "OUB"),
                BoxType.AdditionalUnitBox   => Path.Combine(baseDir, "labels", "AUB"),
                BoxType.AdditionalCartonBox => Path.Combine(baseDir, "labels", "ACB"),
                BoxType.OuterCartonBox      => Path.Combine(baseDir, "labels", "OCB"),
                BoxType.CaseLabel           => Path.Combine(baseDir, "labels", "Case"),
                _                           => Path.Combine(baseDir, "labels", "UB"),
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        //  PRN command builder  (mirrors LabelPrinter.UpdatePrintCommand)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Substitutes the job-ID, sequence number, and quantity fields inside
        /// the raw SATO ESC/P-like command stream.
        /// Mirrors LabelPrinter.UpdatePrintCommand() and the byte-level patching
        /// logic in LabelPrinter.vb.
        /// NOTE: The exact byte offsets are printer-model specific.  Replace the
        ///       string-replacement approach below with the correct binary
        ///       patching logic for your SATO CL612e firmware version.
        /// </summary>
        private static byte[] UpdatePrintCommand(byte[] template,
                                                  string jobId,
                                                  long   sequence,
                                                  long   quantity,
                                                  BoxType labelType)
        {
            // Convert to editable string (SATO commands are ASCII-printable).
            string cmd = System.Text.Encoding.ASCII.GetString(template);

            // Replace job-ID placeholder  (<JOBID> is a documentation convention)
            cmd = cmd.Replace("<JOBID>",    jobId.PadRight(20));

            // Replace sequence placeholder
            cmd = cmd.Replace("<SEQ>",      sequence.ToString().PadLeft(6, '0'));

            // Replace quantity placeholder
            cmd = cmd.Replace("<QTY>",      quantity.ToString().PadLeft(4, '0'));

            return System.Text.Encoding.ASCII.GetBytes(cmd);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Win32 printer send  (mirrors LabelPrinter.SendBytesToPrinter)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Opens the named printer, sends the raw byte buffer, and closes.
        /// Mirrors LabelPrinter.SendBytesToPrinter() / WritePrinter().
        /// </summary>
        private static bool SendBytesToPrinter(string printerName, byte[] data, string docName)
        {
            IntPtr hPrinter = IntPtr.Zero;
            var    docInfo  = new DocInfo1
            {
                pDocName    = docName,
                pOutputFile = null,
                pDataType   = RawDataType,
            };

            if (!OpenPrinter(printerName, ref hPrinter, 0))
                throw new InvalidOperationException(
                    $"Cannot open printer '{printerName}'. " +
                    $"Error {Marshal.GetLastWin32Error()}");
            try
            {
                if (StartDocPrinter(hPrinter, 1, ref docInfo) == 0)
                    throw new InvalidOperationException("StartDocPrinter failed.");

                StartPagePrinter(hPrinter);

                // Pin the managed byte array so we can pass its address to Win32
                GCHandle handle  = GCHandle.Alloc(data, GCHandleType.Pinned);
                IntPtr   pBuffer = handle.AddrOfPinnedObject();
                int      written = 0;
                bool     ok      = WritePrinter(hPrinter, pBuffer, data.Length, ref written);
                handle.Free();

                EndPagePrinter(hPrinter);
                EndDocPrinter(hPrinter);

                return ok && written == data.Length;
            }
            finally
            {
                ClosePrinter(hPrinter);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Analyzer  (mirrors frmCaseQualityCheck / frmCaseResult usage)
        // ─────────────────────────────────────────────────────────────────────

        private void CollectAnalyzerResult(DataRow row, long sequence)
        {
            // TODO: open serial port or TCP connection to the MS3204 analyzer,
            //       send the trigger command, read Pass/Fail, and populate:
            //   row["analyzer_result"] = "Pass" / "Fail"
            //   row["visual_result"]   = ...
            //   row["verifier_result"] = ...
            //
            // For now this is a stub that leaves the columns as DBNull,
            // which SavePrintResult handles by writing NULL to the DB
            // (matching the VB IsDBNull branch in SavePrintResult).
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Result row builder
        // ─────────────────────────────────────────────────────────────────────

        private DataRow BuildResultRow(long sequence, string printerName, bool printed)
        {
            // Ensure ResultData table exists
            if (_printData.ResultData == null)
                _printData.ResultData = CreateResultTable();

            var row = _printData.ResultData.NewRow();
            row["item_code"]       = _printData.ItemCode;
            row["lot_no"]          = _printData.LotNo;
            row["sequence"]        = sequence;
            row["printer_name"]    = printerName;
            row["batch_count"]     = 1;
            row["batch_set"]       = 1;
            row["label_status"]    = printed
                                     ? (int)_printData.LabelStatus
                                     : (int)LabelPrintType.FailedDuringPrinting;
            row["analyzer_result"] = DBNull.Value;
            row["visual_result"]   = DBNull.Value;
            row["verifier_result"] = DBNull.Value;
            row["created_user"]    = _printData.ApprovedUserCode;
            row["created_section"] = _printData.ApprovedSectionId;
            row["paired_type"]     = 0;

            _printData.ResultData.Rows.Add(row);
            return row;
        }

        private static DataTable CreateResultTable()
        {
            var dt = new DataTable("PrintResult");
            foreach (string col in new[]
            {
                "item_code", "lot_no", "sequence", "printer_name",
                "batch_count", "batch_set", "label_status",
                "analyzer_result", "visual_result", "verifier_result",
                "created_user", "created_section", "paired_type",
            })
                dt.Columns.Add(col);
            return dt;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Save results  (mirrors LabelPrinterDB.SavePrintResult)
        // ─────────────────────────────────────────────────────────────────────

        private void SaveSingleResult(DataRow row)
        {
            string printTable = ResolvePrintTable(_printData.LabelType);
            string ip         = GetLocalIp();

            _repo.SavePrintRecord(printTable, row,
                                  _printData.ApprovedUserCode,
                                  _printData.ApprovedSectionId,
                                  ip);
        }

        private static string ResolvePrintTable(BoxType labelType)
        {
            return labelType switch
            {
                BoxType.UnitBox or BoxType.QC or BoxType.McKessonUB
                    => WorkOrderRepository.Tables.UbPrint,
                BoxType.CartonBox or BoxType.COC
                    => WorkOrderRepository.Tables.CbPrint,
                BoxType.OuterUnitBox or BoxType.OubQC
                    => WorkOrderRepository.Tables.OubPrint,
                BoxType.AdditionalUnitBox or BoxType.AubQC
                    => WorkOrderRepository.Tables.AubPrint,
                BoxType.AdditionalCartonBox
                    => WorkOrderRepository.Tables.AcbPrint,
                BoxType.OuterCartonBox
                    => WorkOrderRepository.Tables.OcbPrint,
                BoxType.CaseLabel
                    => WorkOrderRepository.Tables.CasePrint,
                _   => WorkOrderRepository.Tables.UbPrint,
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        //  UI helpers
        // ─────────────────────────────────────────────────────────────────────

        private void AppendLog(string message)
        {
            if (InvokeRequired) { Invoke((Action<string>)AppendLog, message); return; }
            rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            rtbLog.ScrollToCaret();
        }

        private void SetPrintButtonState(bool ready)
        {
            if (InvokeRequired) { Invoke((Action<bool>)SetPrintButtonState, ready); return; }
            btnPrint.Text    = ready ? "Print" : "Pause";
            btnStop.Enabled  = !ready;
            _isPrinting      = !ready;
        }

        private static string GetLocalIp()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        return ip.ToString();
            }
            catch { /* fallback below */ }
            return "127.0.0.1";
        }

        private void WireEvents()
        {
            Load             += LabelPrintForm_Load;
            btnPrint.Click   += BtnPrint_Click;
            btnStop.Click    += BtnStop_Click;
            btnClose.Click   += BtnClose_Click;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Designer-generated / control declarations
        // ─────────────────────────────────────────────────────────────────────

        private void InitializeComponent()
        {
            lblItemCode  = new Label();
            lblLotNo     = new Label();
            lblLabelType = new Label();
            lblStatus    = new Label();
            lblSequence  = new Label();

            lblQtyPrompt = new Label();
            txtQuantity  = new TextBox();

            lblPrinterPrompt = new Label();
            cboPrinter       = new ComboBox();

            chkAnalyzer = new CheckBox();

            btnPrint = new Button();
            btnStop  = new Button();
            btnClose = new Button();

            pgbPrint = new ProgressBar();
            rtbLog   = new RichTextBox();

            SuspendLayout();

            int y = 12;

            // -- header labels
            lblItemCode.Location  = new System.Drawing.Point(12, y); y += 22;
            lblItemCode.AutoSize  = true;
            lblLotNo.Location     = new System.Drawing.Point(12, y); y += 22;
            lblLotNo.AutoSize     = true;
            lblLabelType.Location = new System.Drawing.Point(12, y); y += 22;
            lblLabelType.AutoSize = true;
            lblStatus.Location    = new System.Drawing.Point(12, y); y += 22;
            lblStatus.AutoSize    = true;
            lblSequence.Location  = new System.Drawing.Point(12, y); y += 30;
            lblSequence.AutoSize  = true;

            // -- quantity
            lblQtyPrompt.Text     = "Quantity:";
            lblQtyPrompt.Location = new System.Drawing.Point(12, y);
            lblQtyPrompt.AutoSize = true;
            txtQuantity.Location  = new System.Drawing.Point(100, y - 2);
            txtQuantity.Size      = new System.Drawing.Size(80, 23);
            y += 30;

            // -- printer
            lblPrinterPrompt.Text     = "Printer:";
            lblPrinterPrompt.Location = new System.Drawing.Point(12, y);
            lblPrinterPrompt.AutoSize = true;
            cboPrinter.Location       = new System.Drawing.Point(100, y - 2);
            cboPrinter.Size           = new System.Drawing.Size(300, 23);
            cboPrinter.DropDownStyle  = ComboBoxStyle.DropDownList;
            y += 35;

            // -- analyzer checkbox
            chkAnalyzer.Text     = "Enable Analyzer (MS3204)";
            chkAnalyzer.Location = new System.Drawing.Point(12, y);
            chkAnalyzer.AutoSize = true;
            y += 30;

            // -- buttons
            btnPrint.Text     = "Print";
            btnPrint.Location = new System.Drawing.Point(12, y);
            btnPrint.Size     = new System.Drawing.Size(80, 30);

            btnStop.Text      = "Stop";
            btnStop.Location  = new System.Drawing.Point(105, y);
            btnStop.Size      = new System.Drawing.Size(80, 30);
            btnStop.Enabled   = false;

            btnClose.Text     = "Close";
            btnClose.Location = new System.Drawing.Point(198, y);
            btnClose.Size     = new System.Drawing.Size(80, 30);
            y += 40;

            // -- progress bar
            pgbPrint.Location = new System.Drawing.Point(12, y);
            pgbPrint.Size     = new System.Drawing.Size(460, 18);
            pgbPrint.Maximum  = 100;
            y += 25;

            // -- log
            rtbLog.Location  = new System.Drawing.Point(12, y);
            rtbLog.Size      = new System.Drawing.Size(460, 140);
            rtbLog.ReadOnly  = true;
            rtbLog.BackColor = System.Drawing.Color.Black;
            rtbLog.ForeColor = System.Drawing.Color.Lime;

            // -- Form
            ClientSize = new System.Drawing.Size(490, y + 155);
            Text       = "LAS31 – Label Print";
            Controls.AddRange(new Control[]
            {
                lblItemCode, lblLotNo, lblLabelType, lblStatus, lblSequence,
                lblQtyPrompt, txtQuantity,
                lblPrinterPrompt, cboPrinter,
                chkAnalyzer,
                btnPrint, btnStop, btnClose,
                pgbPrint, rtbLog,
            });

            ResumeLayout(false);
        }

        // ── Control fields ────────────────────────────────────────────────────
        private Label       lblItemCode;
        private Label       lblLotNo;
        private Label       lblLabelType;
        private Label       lblStatus;
        private Label       lblSequence;
        private Label       lblQtyPrompt;
        private TextBox     txtQuantity;
        private Label       lblPrinterPrompt;
        private ComboBox    cboPrinter;
        private CheckBox    chkAnalyzer;
        private Button      btnPrint;
        private Button      btnStop;
        private Button      btnClose;
        private ProgressBar pgbPrint;
        private RichTextBox rtbLog;
    }
}
