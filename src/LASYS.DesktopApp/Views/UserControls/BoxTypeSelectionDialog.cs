using System;
using System.Windows.Forms;
using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Models;

namespace LASYS.DesktopApp.Views.UserControls
{
    public class BoxTypeSelectionDialog : Form
    {
        public BoxTypeSelection Selection { get; private set; }

        private ComboBox cboBoxType;
        private Button btnOk;
        private Button btnCancel;
        private Label lblPrompt;

        public BoxTypeSelectionDialog()
        {
            InitializeComponent();
            // default: show full list
            PopulateOptions(null, null, null, null, null, null);
        }

        // New ctor: accept the instruction codes and only show entries that have a value
        public BoxTypeSelectionDialog(string? ubLiCode, string? aubLiCode, string? oubLiCode, string? cbLiCode, string? acbLiCode, string? ocbLiCode)
        {
            InitializeComponent();
            PopulateOptions(ubLiCode, aubLiCode, oubLiCode, cbLiCode, acbLiCode, ocbLiCode);
        }

        private void InitializeComponent()
        {
            lblPrompt = new Label();
            cboBoxType = new ComboBox();
            btnOk = new Button();
            btnCancel = new Button();

            SuspendLayout();

            // -- lblPrompt
            lblPrompt.Text = "Select Box Type to Print:";
            lblPrompt.Location = new System.Drawing.Point(12, 15);
            lblPrompt.AutoSize = true;

            // -- cboBoxType
            cboBoxType.DropDownStyle = ComboBoxStyle.DropDownList;
            cboBoxType.Location = new System.Drawing.Point(12, 40);
            cboBoxType.Size = new System.Drawing.Size(360, 23);

            // -- btnOk
            btnOk.Text = "OK";
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Location = new System.Drawing.Point(12, 80);
            btnOk.Size = new System.Drawing.Size(75, 28);
            btnOk.Click += BtnOk_Click;

            // -- btnCancel
            btnCancel.Text = "Cancel";
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new System.Drawing.Point(100, 80);
            btnCancel.Size = new System.Drawing.Size(75, 28);

            // -- Form
            AcceptButton = btnOk;
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(390, 125);
            Text = "Box Type Selection";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Controls.AddRange(new Control[] { lblPrompt, cboBoxType, btnOk, btnCancel });

            ResumeLayout(false);
        }

        private void PopulateOptions(string? ubLiCode, string? aubLiCode, string? oubLiCode, string? cbLiCode, string? acbLiCode, string? ocbLiCode)
        {
            cboBoxType.Items.Clear();

            bool anyProvided = !string.IsNullOrWhiteSpace(ubLiCode)
                               || !string.IsNullOrWhiteSpace(aubLiCode)
                               || !string.IsNullOrWhiteSpace(oubLiCode)
                               || !string.IsNullOrWhiteSpace(cbLiCode)
                               || !string.IsNullOrWhiteSpace(acbLiCode)
                               || !string.IsNullOrWhiteSpace(ocbLiCode);

            if (!anyProvided)
            {
                // Fallback to full static list (preserve previous behavior)
                cboBoxType.Items.Add(new ComboEntry("Unit Box (UB)", BoxType.UnitBox));
                cboBoxType.Items.Add(new ComboEntry("Carton Box (CB)", BoxType.CartonBox));
                cboBoxType.Items.Add(new ComboEntry("Outer Unit Box (OUB)", BoxType.OuterUnitBox));
                cboBoxType.Items.Add(new ComboEntry("Additional Unit Box (AUB)", BoxType.AdditionalUnitBox));
                cboBoxType.Items.Add(new ComboEntry("Additional Carton Box (ACB)", BoxType.AdditionalCartonBox));
                cboBoxType.Items.Add(new ComboEntry("Outer Carton Box (OCB)", BoxType.OuterCartonBox));
                cboBoxType.Items.Add(new ComboEntry("Case Label", BoxType.CaseLabel));
                cboBoxType.Items.Add(new ComboEntry("QC Sample", BoxType.QC));
                cboBoxType.Items.Add(new ComboEntry("Certificate of Conformance (COC)", BoxType.COC));
            }
            else
            {
                // Add only the box types that have a non-empty instruction code
                if (!string.IsNullOrWhiteSpace(ubLiCode))
                    cboBoxType.Items.Add(new ComboEntry($"Unit Box (UB) - {ubLiCode}", BoxType.UnitBox));
                if (!string.IsNullOrWhiteSpace(cbLiCode))
                    cboBoxType.Items.Add(new ComboEntry($"Carton Box (CB) - {cbLiCode}", BoxType.CartonBox));
                if (!string.IsNullOrWhiteSpace(oubLiCode))
                    cboBoxType.Items.Add(new ComboEntry($"Outer Unit Box (OUB) - {oubLiCode}", BoxType.OuterUnitBox));
                if (!string.IsNullOrWhiteSpace(aubLiCode))
                    cboBoxType.Items.Add(new ComboEntry($"Additional Unit Box (AUB) - {aubLiCode}", BoxType.AdditionalUnitBox));
                if (!string.IsNullOrWhiteSpace(acbLiCode))
                    cboBoxType.Items.Add(new ComboEntry($"Additional Carton Box (ACB) - {acbLiCode}", BoxType.AdditionalCartonBox));
                if (!string.IsNullOrWhiteSpace(ocbLiCode))
                    cboBoxType.Items.Add(new ComboEntry($"Outer Carton Box (OCB) - {ocbLiCode}", BoxType.OuterCartonBox));
            }

            cboBoxType.SelectedIndex = cboBoxType.Items.Count > 0 ? 0 : -1;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            var entry = cboBoxType.SelectedItem as ComboEntry;
            Selection = new BoxTypeSelection
            {
                BoxType = entry?.Type ?? BoxType.NotSet
            };
        }

        private class ComboEntry
        {
            public string Text { get; }
            public BoxType Type { get; }
            public ComboEntry(string text, BoxType type)
            {
                Text = text;
                Type = type;
            }
            public override string ToString() => Text;
        }
    }
}
