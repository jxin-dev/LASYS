// =============================================================================
// LASYS – Label Printing (C# Port)
// File   : Forms\WorkOrderListingForm.cs
// Purpose: Main work-order listing UI.
//          Mirrors frmWorkOrderListing.vb – barcode scanning, grid display,
//          row selection, and navigation to LabelPrintForm.
// =============================================================================

using System;
using System.Data;
using System.Drawing;
using System.Net;
using System.Windows.Forms;

namespace LASYS.LabelPrinting.Forms
{
    /// <summary>
    /// Work-order listing form.
    /// Mirrors frmWorkOrderListing.vb (LAS31 / LAS32).
    ///
    /// Flow:
    ///   1. Form loads → grid populated via <see cref="LoadWorkOrders"/>.
    ///   2. Operator types/scans a barcode in <see cref="txtInstruction"/> and
    ///      presses Enter → <see cref="TxtInstruction_KeyPress"/> fires.
    ///   3. Service validates/parses the barcode → flags are stored in
    ///      <see cref="_selection"/>.
    ///   4. If IsBarcodeUpdated → <see cref="ProcessData"/> builds a PrintData
    ///      and opens <see cref="LabelPrintForm"/>.
    ///   5. Alternatively, user selects a row in the grid and clicks
    ///      "View / Print" → same ProcessData path.
    /// </summary>
    public partial class WorkOrderListingForm : Form
    {
        // ── dependencies ─────────────────────────────────────────────────────
        private readonly WorkOrderService    _service;
        private readonly WorkOrderRepository _repo;

        // ── state ─────────────────────────────────────────────────────────────
        private bool              _isLabelPrinting = true;   // LAS31 mode
        private BoxTypeSelection  _selection       = null;
        private DataGridViewRow   _selectedRow     = null;
        private bool              _isScannedData   = false;

        // Barcode fields captured from the selected grid row
        private string _ubBarcode  = string.Empty;
        private string _oubBarcode = string.Empty;
        private string _aubBarcode = string.Empty;
        private string _acbBarcode = string.Empty;
        private string _icbBarcode = string.Empty;
        private string _ocbBarcode = string.Empty;
        private string _cbBarcode  = string.Empty;

        // ── column-alias constants (mirror the VB ReadOnly fields) ───────────
        private const string ColItemCode    = "ItemCode";
        private const string ColLotNo       = "LotNo";
        private const string ColUbLiCode    = "UB_LI_Code";
        private const string ColUbPrintType = "UB_PrintType";
        private const string ColCbLiCode    = "CB_LI_Code";
        private const string ColCbPrintType = "CB_PrintType";
        private const string ColOubLiCode   = "OUB_LI_Code";
        private const string ColAubLiCode   = "AUB_LI_Code";
        private const string ColAcbLiCode   = "ACB_LI_Code";
        private const string ColOcbLiCode   = "OCB_LI_Code";

        // ─────────────────────────────────────────────────────────────────────
        //  Constructor
        // ─────────────────────────────────────────────────────────────────────

        public WorkOrderListingForm(WorkOrderService service, WorkOrderRepository repo)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _repo    = repo    ?? throw new ArgumentNullException(nameof(repo));

            InitializeComponent();
            WireEvents();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Form-load
        // ─────────────────────────────────────────────────────────────────────

        private void WorkOrderListingForm_Load(object sender, EventArgs e)
        {
            LoadWorkOrders();
            txtInstruction.Focus();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Grid loader  (mirrors LoadGridViewPane / DisplayObjects in VB)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Fetches the work-order list from the DB and binds it to the grid.
        /// </summary>
        private void LoadWorkOrders()
        {
            try
            {
                DataTable dt = _repo.GetWorkOrders();
                dgvWorkOrders.AutoGenerateColumns = false;
                dgvWorkOrders.DataSource = dt;

                // Colour alternating rows
                dgvWorkOrders.AlternatingRowsDefaultCellStyle.BackColor = Color.AliceBlue;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading work orders:\n{ex.Message}",
                                "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Barcode input / Enter key  (mirrors CheckBarcode in VB)
        // ─────────────────────────────────────────────────────────────────────

        private void TxtInstruction_KeyPress(object sender, KeyPressEventArgs e)
        {
            // ASCII 13 = Enter
            if (e.KeyChar != (char)13) return;

            string barcode = txtInstruction.Text.Trim().ToUpperInvariant()
                                           .Replace("'", string.Empty); // strip single-quotes (Round 49.02 NNP fix)

            if (_isLabelPrinting)
            {
                // Label-printing mode: validate barcode and derive flags
                _selection = _service.BarcodeScan(barcode, out string err);

                if (_selection == null)
                {
                    MessageBox.Show(err ?? "Invalid Label Instruction Code.",
                                    "Scan Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (!_selection.IsBarcodeUpdated)
                {
                    MessageBox.Show(
                        "The inputted Label Instruction is already outdated. " +
                        "Please use the latest Label Instruction.",
                        "Outdated Barcode", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    _isScannedData = true;
                    ProcessData();
                }
            }
            else
            {
                // Status-management mode: trigger status update only
                TriggerBarcode(barcode);
            }

            // Re-select all text so scanner can overwrite immediately
            txtInstruction.SelectAll();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Grid row selection  (mirrors DataGridView_SelectionChanged in VB)
        // ─────────────────────────────────────────────────────────────────────

        private void DgvWorkOrders_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvWorkOrders.CurrentRow == null) return;
            _selectedRow = dgvWorkOrders.CurrentRow;
            CaptureBarcodesFromRow(_selectedRow);
        }

        /// <summary>
        /// Reads the instruction-code columns from the selected row into
        /// the private barcode fields (mirrors the column-reading in VB SetData).
        /// </summary>
        private void CaptureBarcodesFromRow(DataGridViewRow row)
        {
            _ubBarcode  = GetCellString(row, ColUbLiCode);
            _cbBarcode  = GetCellString(row, ColCbLiCode);
            _oubBarcode = GetCellString(row, ColOubLiCode);
            _aubBarcode = GetCellString(row, ColAubLiCode);
            _acbBarcode = GetCellString(row, ColAcbLiCode);
            _ocbBarcode = GetCellString(row, ColOcbLiCode);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  View/Print button  (mirrors btnViewPrint_Click in VB)
        // ─────────────────────────────────────────────────────────────────────

        private void BtnViewPrint_Click(object sender, EventArgs e)
        {
            _isScannedData = false;

            if (_selectedRow == null)
            {
                MessageBox.Show("Please select a record.",
                                "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // User must choose box type when selecting from grid (BoxType dialog)
            var boxTypeResult = ShowBoxTypeDialog();
            if (boxTypeResult == null) return;   // user cancelled

            _selection              = boxTypeResult;
            _selection.ItemCode     = GetCellString(_selectedRow, ColItemCode);
            _selection.LotNo        = GetCellString(_selectedRow, ColLotNo);

            ProcessData();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Refresh button
        // ─────────────────────────────────────────────────────────────────────

        private void BtnRefresh_Click(object sender, EventArgs e) => LoadWorkOrders();

        // ─────────────────────────────────────────────────────────────────────
        //  ProcessData  (mirrors frmWorkOrderListing.ProcessData in VB)
        //  This is the core method that validates, builds PrintData, and
        //  opens the LabelPrintForm.
        // ─────────────────────────────────────────────────────────────────────

        private void ProcessData()
        {
            try
            {
                SetCursorBusy(true);
                UpdateProgress(35);

                // ── 1. Resolve which box type / label type to print
                var labelType = ResolveLabelType();
                if (labelType == BoxType.NotSet) return;

                // ── 2. Build the PrintData object
                UpdateProgress(45);
                var printData = BuildPrintData(labelType);
                if (printData == null) return;

                // ── 3. Validate user/computer section access
                UpdateProgress(50);
                if (!ValidateUserAccess(printData)) return;

                // ── 4. Open the label-print form
                UpdateProgress(60);
                OpenLabelPrintForm(printData);

                UpdateProgress(100);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during processing:\n{ex.Message}",
                                "Processing Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetCursorBusy(false);
                ResetProgress();
            }
        }

        // ── ResolveLabelType  (mirrors CheckLabelType in VB) ─────────────────

        private BoxType ResolveLabelType()
        {
            if (_isScannedData && _selection != null)
                return _selection.BoxType;

            if (_selectedRow == null) return BoxType.NotSet;

            // If a grid row is selected, the box type is determined by which
            // instruction-code columns are populated (UB vs CB vs OUB, etc.).
            if (!string.IsNullOrEmpty(_ubBarcode))  return BoxType.UnitBox;
            if (!string.IsNullOrEmpty(_cbBarcode))  return BoxType.CartonBox;
            if (!string.IsNullOrEmpty(_oubBarcode)) return BoxType.OuterUnitBox;
            if (!string.IsNullOrEmpty(_aubBarcode)) return BoxType.AdditionalUnitBox;
            if (!string.IsNullOrEmpty(_acbBarcode)) return BoxType.AdditionalCartonBox;
            if (!string.IsNullOrEmpty(_ocbBarcode)) return BoxType.OuterCartonBox;

            return BoxType.NotSet;
        }

        // ── BuildPrintData  (mirrors SetData + GetData in VB) ─────────────────

        private PrintData BuildPrintData(BoxType labelType)
        {
            PrintData pd;

            if (_isScannedData && _selection != null)
            {
                pd = _service.MapSelectionToPrintData(_selection);
                pd.LabelType = labelType;
            }
            else
            {
                if (_selectedRow == null)
                {
                    MessageBox.Show("Please select a record.",
                                    "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }

                string itemCode = GetCellString(_selectedRow, ColItemCode);
                string lotNo    = GetCellString(_selectedRow, ColLotNo);
                string insCode  = ResolveInstructionCode(labelType);
                var    status   = ResolveLabelStatus(labelType);

                pd = _service.BuildPrintData(itemCode, lotNo, insCode, labelType, status);
            }

            return pd;
        }

        private string ResolveInstructionCode(BoxType labelType)
        {
            return labelType switch
            {
                BoxType.UnitBox or BoxType.CaseLabel or BoxType.QC
                    => _ubBarcode,
                BoxType.AdditionalUnitBox or BoxType.AubQC
                    => _aubBarcode,
                BoxType.AdditionalCartonBox
                    => _acbBarcode,
                BoxType.OuterUnitBox or BoxType.OubQC
                    => _oubBarcode,
                BoxType.OuterCartonBox
                    => _ocbBarcode,
                BoxType.CartonBox or BoxType.COC
                    => _cbBarcode,
                _   => _ubBarcode,
            };
        }

        private LabelPrintType ResolveLabelStatus(BoxType labelType)
        {
            if (_selectedRow == null) return LabelPrintType.Original;

            string printTypeCell = labelType switch
            {
                BoxType.CartonBox => GetCellString(_selectedRow, ColCbPrintType),
                _                 => GetCellString(_selectedRow, ColUbPrintType),
            };

            if (int.TryParse(printTypeCell, out int pt))
                return (LabelPrintType)pt;

            return LabelPrintType.Original;
        }

        // ── ValidateUserAccess  (mirrors UserValidation in VB) ─────────────────

        private bool ValidateUserAccess(PrintData pd)
        {
            // TODO: query pre_tpc_products_mst for allowed sections,
            //       compare against the logged-in user's section.
            //       Return false (with a message) if not authorised.
            return true; // placeholder: always authorised
        }

        // ── Open the label-print form ──────────────────────────────────────────

        private void OpenLabelPrintForm(PrintData printData)
        {
            var lpForm = new LabelPrintForm(_repo, printData);
            lpForm.MdiParent = this.MdiParent;  // keep MDI hierarchy
            lpForm.Dock      = DockStyle.Fill;
            lpForm.Show();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  TriggerBarcode  (status-management mode – mirrors TriggerBarcode VB)
        // ─────────────────────────────────────────────────────────────────────

        private void TriggerBarcode(string barcode)
        {
            // Status-management mode populates the grid filtered by the scanned
            // barcode rather than navigating to printing.
            var selection = _service.BarcodeScan(barcode, out string err);
            if (selection == null)
            {
                MessageBox.Show(err ?? "Invalid barcode.", "Scan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            FilterGridByBarcode(selection.ItemCode, selection.LotNo);
        }

        private void FilterGridByBarcode(string itemCode, string lotNo)
        {
            if (dgvWorkOrders.DataSource is DataTable dt)
            {
                dt.DefaultView.RowFilter =
                    $"ItemCode = '{itemCode.Replace("'", "''")}' " +
                    $"AND LotNo = '{lotNo.Replace("'", "''")}'";
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Box-type selection dialog  (mirrors frmBoxType usage in VB)
        // ─────────────────────────────────────────────────────────────────────

        private BoxTypeSelection ShowBoxTypeDialog()
        {
            using var dlg = new BoxTypeSelectionDialog();
            if (dlg.ShowDialog(this) == DialogResult.OK)
                return dlg.Selection;
            return null;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  UI helpers
        // ─────────────────────────────────────────────────────────────────────

        private static string GetCellString(DataGridViewRow row, string columnName)
        {
            if (row.DataGridView.Columns[columnName] == null) return string.Empty;
            return row.Cells[columnName].Value?.ToString() ?? string.Empty;
        }

        private void SetCursorBusy(bool busy)
        {
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
            btnViewPrint.Enabled = !busy;
        }

        private void UpdateProgress(int value)
        {
            if (pgbSave.IsHandleCreated)
            {
                pgbSave.Visible = true;
                pgbSave.Value   = Math.Min(value, pgbSave.Maximum);
            }
        }

        private void ResetProgress()
        {
            pgbSave.Visible = false;
            pgbSave.Value   = 0;
        }

        private void WireEvents()
        {
            Load                               += WorkOrderListingForm_Load;
            txtInstruction.KeyPress            += TxtInstruction_KeyPress;
            dgvWorkOrders.SelectionChanged     += DgvWorkOrders_SelectionChanged;
            btnViewPrint.Click                 += BtnViewPrint_Click;
            btnRefresh.Click                   += BtnRefresh_Click;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Designer-generated / control declarations
        //  (In production these live in the .Designer.cs partial class.)
        // ─────────────────────────────────────────────────────────────────────

        private void InitializeComponent()
        {
            this.txtInstruction = new TextBox();
            this.dgvWorkOrders  = new DataGridView();
            this.btnViewPrint   = new Button();
            this.btnRefresh     = new Button();
            this.pgbSave        = new ProgressBar();
            this.lblInstruction = new Label();

            SuspendLayout();

            // -- lblInstruction
            lblInstruction.Text     = "Scan / Enter Label Instruction:";
            lblInstruction.Location = new Point(12, 15);
            lblInstruction.AutoSize = true;

            // -- txtInstruction
            txtInstruction.Location = new Point(230, 12);
            txtInstruction.Size     = new Size(260, 23);
            txtInstruction.CharacterCasing = CharacterCasing.Upper;

            // -- dgvWorkOrders
            dgvWorkOrders.Location         = new Point(12, 48);
            dgvWorkOrders.Size             = new Size(960, 480);
            dgvWorkOrders.ReadOnly         = true;
            dgvWorkOrders.SelectionMode    = DataGridViewSelectionMode.FullRowSelect;
            dgvWorkOrders.MultiSelect      = false;
            dgvWorkOrders.AllowUserToAddRows    = false;
            dgvWorkOrders.AllowUserToDeleteRows = false;
            dgvWorkOrders.AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.AllCells;

            // grid columns
            AddGridColumn("ItemCode",       "Item Code");
            AddGridColumn("LotNo",          "Lot No");
            AddGridColumn("ExpiryDate",     "Exp. Date");
            AddGridColumn("ProdQty",        "Prod Qty");
            AddGridColumn("UB_LI_Code",     "UB LI Code");
            AddGridColumn("UB_Qty",         "UB Qty");
            AddGridColumn("UB_PrintType",   "UB Print Type");
            AddGridColumn("UB_Verdict",     "UB Verdict");
            AddGridColumn("UB_LI_Status",   "UB LI Status");
            AddGridColumn("UB_LabelStatus", "UB Label Status");
            AddGridColumn("CB_LI_Code",     "CB LI Code");
            AddGridColumn("CB_Qty",         "CB Qty");
            AddGridColumn("CB_PrintType",   "CB Print Type");
            AddGridColumn("CB_Verdict",     "CB Verdict");
            AddGridColumn("CB_LI_Status",   "CB LI Status");
            AddGridColumn("CB_LabelStatus", "CB Label Status");

            // -- btnViewPrint
            btnViewPrint.Text     = "View / Print";
            btnViewPrint.Location = new Point(12, 540);
            btnViewPrint.Size     = new Size(110, 30);

            // -- btnRefresh
            btnRefresh.Text     = "Refresh";
            btnRefresh.Location = new Point(135, 540);
            btnRefresh.Size     = new Size(80, 30);

            // -- pgbSave
            pgbSave.Location = new Point(12, 580);
            pgbSave.Size     = new Size(400, 20);
            pgbSave.Visible  = false;
            pgbSave.Maximum  = 100;

            // -- Form
            ClientSize = new Size(990, 615);
            Text       = "LAS31 – Work Order Listing (Label Printing)";
            Controls.AddRange(new Control[]
            {
                lblInstruction, txtInstruction,
                dgvWorkOrders,
                btnViewPrint, btnRefresh,
                pgbSave
            });

            ResumeLayout(false);
        }

        private void AddGridColumn(string name, string header)
        {
            dgvWorkOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = name,
                Name             = name,
                HeaderText       = header,
            });
        }

        // ── Control fields ────────────────────────────────────────────────────
        private TextBox     txtInstruction;
        private DataGridView dgvWorkOrders;
        private Button      btnViewPrint;
        private Button      btnRefresh;
        private ProgressBar pgbSave;
        private Label       lblInstruction;
    }
}
