namespace LASYS.DesktopApp.Views.UserControls
{
    partial class PrinterManagementControl
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
            button1 = new Button();
            btnSavePrinter = new Button();
            cbxPort = new ComboBox();
            lblPort = new Label();
            cbxInterface = new ComboBox();
            label1 = new Label();
            label5 = new Label();
            label4 = new Label();
            lblPrinterState = new Label();
            pnlContent.SuspendLayout();
            SuspendLayout();
            // 
            // pnlContent
            // 
            pnlContent.Anchor = AnchorStyles.None;
            pnlContent.BackColor = Color.White;
            pnlContent.Controls.Add(lblPrinterState);
            pnlContent.Controls.Add(button1);
            pnlContent.Controls.Add(btnSavePrinter);
            pnlContent.Controls.Add(cbxPort);
            pnlContent.Controls.Add(lblPort);
            pnlContent.Controls.Add(cbxInterface);
            pnlContent.Controls.Add(label1);
            pnlContent.Controls.Add(label5);
            pnlContent.Controls.Add(label4);
            pnlContent.Location = new Point(52, 83);
            pnlContent.Name = "pnlContent";
            pnlContent.Size = new Size(745, 286);
            pnlContent.TabIndex = 2;
            // 
            // button1
            // 
            button1.BackColor = Color.DimGray;
            button1.FlatAppearance.BorderSize = 0;
            button1.FlatStyle = FlatStyle.Flat;
            button1.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold);
            button1.ForeColor = Color.White;
            button1.Image = Properties.Resources.print24;
            button1.Location = new Point(210, 206);
            button1.Name = "button1";
            button1.Size = new Size(122, 37);
            button1.TabIndex = 13;
            button1.Text = "Test Print";
            button1.TextImageRelation = TextImageRelation.ImageBeforeText;
            button1.UseVisualStyleBackColor = false;
            // 
            // btnSavePrinter
            // 
            btnSavePrinter.BackColor = SystemColors.HotTrack;
            btnSavePrinter.FlatAppearance.BorderSize = 0;
            btnSavePrinter.FlatStyle = FlatStyle.Flat;
            btnSavePrinter.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold);
            btnSavePrinter.ForeColor = Color.White;
            btnSavePrinter.Image = Properties.Resources.check_small24;
            btnSavePrinter.Location = new Point(21, 206);
            btnSavePrinter.Name = "btnSavePrinter";
            btnSavePrinter.Size = new Size(183, 37);
            btnSavePrinter.TabIndex = 12;
            btnSavePrinter.Text = "Save Printer Settings";
            btnSavePrinter.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnSavePrinter.UseVisualStyleBackColor = false;
            // 
            // cbxPort
            // 
            cbxPort.DropDownStyle = ComboBoxStyle.DropDownList;
            cbxPort.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            cbxPort.FormattingEnabled = true;
            cbxPort.Location = new Point(21, 162);
            cbxPort.Name = "cbxPort";
            cbxPort.Size = new Size(600, 29);
            cbxPort.TabIndex = 11;
            // 
            // lblPort
            // 
            lblPort.AutoSize = true;
            lblPort.Location = new Point(21, 144);
            lblPort.Name = "lblPort";
            lblPort.Size = new Size(12, 15);
            lblPort.TabIndex = 10;
            lblPort.Text = "-";
            // 
            // cbxInterface
            // 
            cbxInterface.DropDownStyle = ComboBoxStyle.DropDownList;
            cbxInterface.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            cbxInterface.FormattingEnabled = true;
            cbxInterface.Items.AddRange(new object[] { "USB Port", "Serial COM" });
            cbxInterface.Location = new Point(21, 112);
            cbxInterface.Name = "cbxInterface";
            cbxInterface.Size = new Size(180, 29);
            cbxInterface.TabIndex = 9;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.FromArgb(240, 84, 84);
            label1.Location = new Point(14, 11);
            label1.Name = "label1";
            label1.Size = new Size(216, 30);
            label1.TabIndex = 0;
            label1.Text = "Printer Management";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(21, 44);
            label5.Name = "label5";
            label5.Size = new Size(703, 30);
            label5.TabIndex = 8;
            label5.Text = "Select the printer interface (USB or Serial COM), choose the appropriate port, click \"Save Printer Settings\" to apply the configuration, \r\nand then use \"Test Print\" to verify the connection.";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(21, 94);
            label4.Name = "label4";
            label4.Size = new Size(91, 15);
            label4.TabIndex = 7;
            label4.Text = "Printer Interface";
            // 
            // lblPrinterState
            // 
            lblPrinterState.Location = new Point(21, 246);
            lblPrinterState.Name = "lblPrinterState";
            lblPrinterState.Size = new Size(703, 34);
            lblPrinterState.TabIndex = 14;
            lblPrinterState.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // PrinterManagementControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(pnlContent);
            Name = "PrinterManagementControl";
            Size = new Size(864, 495);
            pnlContent.ResumeLayout(false);
            pnlContent.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlContent;
        private Label label1;
        private Label label5;
        private Label label4;
        private ComboBox cbxInterface;
        private ComboBox cbxPort;
        private Label lblPort;
        private Button button1;
        private Button btnSavePrinter;
        private Label lblPrinterState;
    }
}
