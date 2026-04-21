// =============================================================================
// LASYS – Label Printing (C# Port)
// File   : Forms\BoxTypeSelectionDialog.cs
// Purpose: Simple dialog that lets the operator choose the box-type (UB / CB /
//          OUB / AUB / ACB / OCB / Case) when selecting from the grid.
//          Mirrors frmBoxType.vb.
// =============================================================================

using System;
using System.Windows.Forms;

namespace LASYS.LabelPrinting.Forms
{
    /// <summary>
    /// Mirrors frmBoxType.vb – prompts the operator to pick the box type
    /// that should be printed for the selected work-order row.
    /// </summary>
    public class BoxTypeSelectionDialog : Form
    {
        public BoxTypeSelection Selection { get; private set; }

        private ComboBox  cboBoxType;
        private Button    btnOk;
        private Button    btnCancel;
        private Label     lblPrompt;

        public BoxTypeSelectionDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            lblPrompt  = new Label();
            cboBoxType = new ComboBox();
            btnOk      = new Button();
            btnCancel  = new Button();

            SuspendLayout();

            // -- lblPrompt
            lblPrompt.Text     = "Select Box Type to Print:";
            lblPrompt.Location = new System.Drawing.Point(12, 15);
            lblPrompt.AutoSize = true;

            // -- cboBoxType
            cboBoxType.DropDownStyle = ComboBoxStyle.DropDownList;
            cboBoxType.Location      = new System.Drawing.Point(12, 40);
            cboBoxType.Size          = new System.Drawing.Size(260, 23);
            cboBoxType.Items.AddRange(new object[]
            {
                "Unit Box (UB)",
                "Carton Box (CB)",
                "Outer Unit Box (OUB)",
                "Additional Unit Box (AUB)",
                "Additional Carton Box (ACB)",
                "Outer Carton Box (OCB)",
                "Case Label",
                "QC Sample",
                "Certificate of Conformance (COC)",
            });
            cboBoxType.SelectedIndex = 0;

            // -- btnOk
            btnOk.Text         = "OK";
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Location     = new System.Drawing.Point(12, 80);
            btnOk.Size         = new System.Drawing.Size(75, 28);
            btnOk.Click       += BtnOk_Click;

            // -- btnCancel
            btnCancel.Text         = "Cancel";
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location     = new System.Drawing.Point(100, 80);
            btnCancel.Size         = new System.Drawing.Size(75, 28);

            // -- Form
            AcceptButton = btnOk;
            CancelButton = btnCancel;
            ClientSize   = new System.Drawing.Size(290, 125);
            Text         = "Box Type Selection";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Controls.AddRange(new Control[] { lblPrompt, cboBoxType, btnOk, btnCancel });

            ResumeLayout(false);
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            Selection = new BoxTypeSelection
            {
                BoxType = cboBoxType.SelectedIndex switch
                {
                    0 => BoxType.UnitBox,
                    1 => BoxType.CartonBox,
                    2 => BoxType.OuterUnitBox,
                    3 => BoxType.AdditionalUnitBox,
                    4 => BoxType.AdditionalCartonBox,
                    5 => BoxType.OuterCartonBox,
                    6 => BoxType.CaseLabel,
                    7 => BoxType.QC,
                    8 => BoxType.COC,
                    _ => BoxType.NotSet,
                }
            };
        }
    }
}
