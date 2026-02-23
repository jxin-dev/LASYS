namespace LASYS.DesktopApp.Views.UserControls
{
    partial class BarcodeDeviceSetupControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pnlContent = new Panel();
            lblBarcodeStatus = new Label();
            btnSetManualMode = new Button();
            btnSaveBarcode = new Button();
            cbxPort = new ComboBox();
            label1 = new Label();
            label5 = new Label();
            label4 = new Label();
            pnlContent.SuspendLayout();
            SuspendLayout();
            // 
            // pnlContent
            // 
            pnlContent.Anchor = AnchorStyles.None;
            pnlContent.BackColor = Color.White;
            pnlContent.Controls.Add(lblBarcodeStatus);
            pnlContent.Controls.Add(btnSetManualMode);
            pnlContent.Controls.Add(btnSaveBarcode);
            pnlContent.Controls.Add(cbxPort);
            pnlContent.Controls.Add(label1);
            pnlContent.Controls.Add(label5);
            pnlContent.Controls.Add(label4);
            pnlContent.Location = new Point(79, 85);
            pnlContent.Name = "pnlContent";
            pnlContent.Size = new Size(585, 235);
            pnlContent.TabIndex = 3;
            // 
            // lblBarcodeStatus
            // 
            lblBarcodeStatus.Location = new Point(21, 187);
            lblBarcodeStatus.Name = "lblBarcodeStatus";
            lblBarcodeStatus.Size = new Size(544, 34);
            lblBarcodeStatus.TabIndex = 14;
            lblBarcodeStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnSetManualMode
            // 
            btnSetManualMode.BackColor = Color.DimGray;
            btnSetManualMode.FlatAppearance.BorderSize = 0;
            btnSetManualMode.FlatStyle = FlatStyle.Flat;
            btnSetManualMode.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold);
            btnSetManualMode.ForeColor = Color.White;
            btnSetManualMode.Image = Properties.Resources.rule_settings24;
            btnSetManualMode.Location = new Point(210, 147);
            btnSetManualMode.Name = "btnSetManualMode";
            btnSetManualMode.Size = new Size(170, 37);
            btnSetManualMode.TabIndex = 13;
            btnSetManualMode.Text = "Set Manual Mode";
            btnSetManualMode.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnSetManualMode.UseVisualStyleBackColor = false;
            // 
            // btnSaveBarcode
            // 
            btnSaveBarcode.BackColor = SystemColors.HotTrack;
            btnSaveBarcode.FlatAppearance.BorderSize = 0;
            btnSaveBarcode.FlatStyle = FlatStyle.Flat;
            btnSaveBarcode.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold);
            btnSaveBarcode.ForeColor = Color.White;
            btnSaveBarcode.Image = Properties.Resources.check_small24;
            btnSaveBarcode.Location = new Point(21, 147);
            btnSaveBarcode.Name = "btnSaveBarcode";
            btnSaveBarcode.Size = new Size(183, 37);
            btnSaveBarcode.TabIndex = 12;
            btnSaveBarcode.Text = "Save Configuration";
            btnSaveBarcode.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnSaveBarcode.UseVisualStyleBackColor = false;
            // 
            // cbxPort
            // 
            cbxPort.DropDownStyle = ComboBoxStyle.DropDownList;
            cbxPort.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            cbxPort.FormattingEnabled = true;
            cbxPort.Location = new Point(21, 112);
            cbxPort.Name = "cbxPort";
            cbxPort.Size = new Size(183, 29);
            cbxPort.TabIndex = 9;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.FromArgb(240, 84, 84);
            label1.Location = new Point(14, 11);
            label1.Name = "label1";
            label1.Size = new Size(225, 30);
            label1.TabIndex = 0;
            label1.Text = "Barcode Device Setup";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(21, 44);
            label5.Name = "label5";
            label5.Size = new Size(536, 30);
            label5.TabIndex = 8;
            label5.Text = "Choose the scanner’s USB Virtual COM port and click Save Configuration to initialize the connection.\r\nUse Set Manual Mode to switch the scanner from continuous scanning to manual trigger mode.";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(21, 94);
            label4.Name = "label4";
            label4.Size = new Size(121, 15);
            label4.TabIndex = 7;
            label4.Text = "USB Virtual COM Port";
            // 
            // BarcodeDeviceSetupControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(pnlContent);
            Name = "BarcodeDeviceSetupControl";
            Size = new Size(749, 413);
            pnlContent.ResumeLayout(false);
            pnlContent.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlContent;
        private Label lblBarcodeStatus;
        private Button btnSetManualMode;
        private Button btnSaveBarcode;
        private ComboBox cbxPort;
        private Label label1;
        private Label label5;
        private Label label4;
    }
}
