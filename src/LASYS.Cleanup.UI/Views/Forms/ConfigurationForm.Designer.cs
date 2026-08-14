namespace LASYS.Cleanup.UI.Views.Forms
{
    partial class ConfigurationForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigurationForm));
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            txtPrintJobFolder = new TextBox();
            btnBrowse = new Button();
            label4 = new Label();
            label5 = new Label();
            groupBox1 = new GroupBox();
            btnCancel = new Button();
            btnSaveSettings = new Button();
            panel2 = new Panel();
            lblScheduleStatus = new Label();
            label12 = new Label();
            cmbRunTime = new ComboBox();
            label11 = new Label();
            cmbFrequency = new ComboBox();
            label9 = new Label();
            label10 = new Label();
            panel1 = new Panel();
            lblRetentionStatus = new Label();
            cmbRetentionUnit = new ComboBox();
            nudRetention = new NumericUpDown();
            label6 = new Label();
            label7 = new Label();
            tableLayoutPanel1 = new TableLayoutPanel();
            panel3 = new Panel();
            pictureBox1 = new PictureBox();
            groupBox1.SuspendLayout();
            panel2.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudRetention).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.MidnightBlue;
            label1.Location = new Point(21, 263);
            label1.Name = "label1";
            label1.Size = new Size(233, 32);
            label1.TabIndex = 0;
            label1.Text = "Automatic Cleanup";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(15, 19);
            label2.Name = "label2";
            label2.Size = new Size(182, 21);
            label2.TabIndex = 1;
            label2.Text = "Schedule Configuration";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.Location = new Point(15, 48);
            label3.Name = "label3";
            label3.Size = new Size(86, 15);
            label3.TabIndex = 2;
            label3.Text = "Cleanup Folder";
            // 
            // txtPrintJobFolder
            // 
            txtPrintJobFolder.ForeColor = Color.FromArgb(64, 64, 64);
            txtPrintJobFolder.Location = new Point(15, 81);
            txtPrintJobFolder.Name = "txtPrintJobFolder";
            txtPrintJobFolder.ReadOnly = true;
            txtPrintJobFolder.Size = new Size(345, 23);
            txtPrintJobFolder.TabIndex = 3;
            // 
            // btnBrowse
            // 
            btnBrowse.Image = Properties.Resources.folder_16dp_000000_FILL0_wght400_GRAD0_opsz20;
            btnBrowse.Location = new Point(366, 78);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(87, 29);
            btnBrowse.TabIndex = 4;
            btnBrowse.Text = "Browse...";
            btnBrowse.TextAlign = ContentAlignment.MiddleRight;
            btnBrowse.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.ForeColor = Color.Gray;
            label4.Location = new Point(9, 295);
            label4.Name = "label4";
            label4.Size = new Size(251, 30);
            label4.TabIndex = 5;
            label4.Text = "Configure and manage the automatic cleanup\r\nof old print job files.\r\n";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.ForeColor = Color.Gray;
            label5.Location = new Point(15, 63);
            label5.Name = "label5";
            label5.Size = new Size(257, 15);
            label5.TabIndex = 6;
            label5.Text = "Select the folder where print job files are stored.";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(btnCancel);
            groupBox1.Controls.Add(btnSaveSettings);
            groupBox1.Controls.Add(panel2);
            groupBox1.Controls.Add(label12);
            groupBox1.Controls.Add(cmbRunTime);
            groupBox1.Controls.Add(label11);
            groupBox1.Controls.Add(cmbFrequency);
            groupBox1.Controls.Add(label9);
            groupBox1.Controls.Add(label10);
            groupBox1.Controls.Add(panel1);
            groupBox1.Controls.Add(cmbRetentionUnit);
            groupBox1.Controls.Add(nudRetention);
            groupBox1.Controls.Add(label6);
            groupBox1.Controls.Add(label7);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(txtPrintJobFolder);
            groupBox1.Controls.Add(btnBrowse);
            groupBox1.Location = new Point(300, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(467, 401);
            groupBox1.TabIndex = 7;
            groupBox1.TabStop = false;
            // 
            // btnCancel
            // 
            btnCancel.BackColor = Color.Silver;
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.ForeColor = Color.Black;
            btnCancel.Image = Properties.Resources.close_16dp_000000_FILL1_wght400_GRAD0_opsz20;
            btnCancel.Location = new Point(125, 351);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(90, 39);
            btnCancel.TabIndex = 20;
            btnCancel.Text = "Cancel";
            btnCancel.TextAlign = ContentAlignment.MiddleRight;
            btnCancel.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnCancel.UseVisualStyleBackColor = false;
            // 
            // btnSaveSettings
            // 
            btnSaveSettings.BackColor = SystemColors.HotTrack;
            btnSaveSettings.FlatAppearance.BorderSize = 0;
            btnSaveSettings.FlatStyle = FlatStyle.Flat;
            btnSaveSettings.ForeColor = Color.White;
            btnSaveSettings.Image = Properties.Resources.save_16dp_FFFFFF_FILL1_wght400_GRAD0_opsz20;
            btnSaveSettings.Location = new Point(15, 351);
            btnSaveSettings.Name = "btnSaveSettings";
            btnSaveSettings.Size = new Size(104, 39);
            btnSaveSettings.TabIndex = 19;
            btnSaveSettings.Text = "Save Settings";
            btnSaveSettings.TextAlign = ContentAlignment.MiddleRight;
            btnSaveSettings.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnSaveSettings.UseVisualStyleBackColor = false;
            btnSaveSettings.Click += btnSaveSettings_Click;
            // 
            // panel2
            // 
            panel2.BackColor = Color.AliceBlue;
            panel2.BorderStyle = BorderStyle.FixedSingle;
            panel2.Controls.Add(lblScheduleStatus);
            panel2.Location = new Point(15, 298);
            panel2.Name = "panel2";
            panel2.Size = new Size(438, 37);
            panel2.TabIndex = 18;
            // 
            // lblScheduleStatus
            // 
            lblScheduleStatus.ForeColor = Color.Gray;
            lblScheduleStatus.Image = Properties.Resources.info_16dp_2563EB_FILL1_wght400_GRAD0_opsz20;
            lblScheduleStatus.ImageAlign = ContentAlignment.MiddleLeft;
            lblScheduleStatus.Location = new Point(3, 7);
            lblScheduleStatus.Name = "lblScheduleStatus";
            lblScheduleStatus.Size = new Size(417, 23);
            lblScheduleStatus.TabIndex = 0;
            lblScheduleStatus.Text = "      Cleanup will run every day at 2:00AM";
            lblScheduleStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.ForeColor = Color.Black;
            label12.Location = new Point(142, 245);
            label12.Name = "label12";
            label12.Size = new Size(34, 15);
            label12.TabIndex = 17;
            label12.Text = "Time";
            // 
            // cmbRunTime
            // 
            cmbRunTime.DropDownHeight = 50;
            cmbRunTime.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRunTime.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            cmbRunTime.FormattingEnabled = true;
            cmbRunTime.IntegralHeight = false;
            cmbRunTime.Location = new Point(142, 263);
            cmbRunTime.Name = "cmbRunTime";
            cmbRunTime.Size = new Size(121, 29);
            cmbRunTime.TabIndex = 16;
            cmbRunTime.SelectedIndexChanged += cmbRunTime_SelectedIndexChanged;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.ForeColor = Color.Black;
            label11.Location = new Point(15, 245);
            label11.Name = "label11";
            label11.Size = new Size(62, 15);
            label11.TabIndex = 15;
            label11.Text = "Frequency";
            // 
            // cmbFrequency
            // 
            cmbFrequency.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFrequency.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            cmbFrequency.FormattingEnabled = true;
            cmbFrequency.Items.AddRange(new object[] { "Daily", "Monthly" });
            cmbFrequency.Location = new Point(15, 263);
            cmbFrequency.Name = "cmbFrequency";
            cmbFrequency.Size = new Size(121, 29);
            cmbFrequency.TabIndex = 14;
            cmbFrequency.SelectedIndexChanged += cmbFrequency_SelectedIndexChanged;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.ForeColor = Color.Gray;
            label9.Location = new Point(15, 230);
            label9.Name = "label9";
            label9.Size = new Size(285, 15);
            label9.TabIndex = 13;
            label9.Text = "Set the time when cleanup should run automatically.\r\n";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label10.Location = new Point(15, 215);
            label10.Name = "label10";
            label10.Size = new Size(56, 15);
            label10.TabIndex = 12;
            label10.Text = "Schedule";
            // 
            // panel1
            // 
            panel1.BackColor = Color.AliceBlue;
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(lblRetentionStatus);
            panel1.Location = new Point(15, 175);
            panel1.Name = "panel1";
            panel1.Size = new Size(438, 37);
            panel1.TabIndex = 11;
            // 
            // lblRetentionStatus
            // 
            lblRetentionStatus.ForeColor = Color.Gray;
            lblRetentionStatus.Image = Properties.Resources.info_16dp_2563EB_FILL1_wght400_GRAD0_opsz20;
            lblRetentionStatus.ImageAlign = ContentAlignment.MiddleLeft;
            lblRetentionStatus.Location = new Point(3, 7);
            lblRetentionStatus.Name = "lblRetentionStatus";
            lblRetentionStatus.Size = new Size(430, 23);
            lblRetentionStatus.TabIndex = 0;
            lblRetentionStatus.Text = "      Files older than 3 months will be deleted during cleanup.";
            lblRetentionStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // cmbRetentionUnit
            // 
            cmbRetentionUnit.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRetentionUnit.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            cmbRetentionUnit.FormattingEnabled = true;
            cmbRetentionUnit.Items.AddRange(new object[] { "Minutes", "Hours", "Days", "Months" });
            cmbRetentionUnit.Location = new Point(80, 140);
            cmbRetentionUnit.Name = "cmbRetentionUnit";
            cmbRetentionUnit.Size = new Size(121, 29);
            cmbRetentionUnit.TabIndex = 10;
            cmbRetentionUnit.SelectedIndexChanged += cmbRetentionUnit_SelectedIndexChanged;
            // 
            // nudRetention
            // 
            nudRetention.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            nudRetention.Location = new Point(15, 140);
            nudRetention.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudRetention.Name = "nudRetention";
            nudRetention.Size = new Size(59, 29);
            nudRetention.TabIndex = 9;
            nudRetention.Value = new decimal(new int[] { 1, 0, 0, 0 });
            nudRetention.ValueChanged += nudRetention_ValueChanged;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.ForeColor = Color.Gray;
            label6.Location = new Point(15, 122);
            label6.Name = "label6";
            label6.Size = new Size(262, 15);
            label6.TabIndex = 8;
            label6.Text = "Delete files older than specified retention period.\r\n";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label7.Location = new Point(15, 107);
            label7.Name = "label7";
            label7.Size = new Size(104, 15);
            label7.TabIndex = 7;
            label7.Text = "Retention Settings";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38.5733147F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 61.4266853F));
            tableLayoutPanel1.Controls.Add(groupBox1, 1, 0);
            tableLayoutPanel1.Controls.Add(panel3, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(770, 411);
            tableLayoutPanel1.TabIndex = 8;
            // 
            // panel3
            // 
            panel3.BackColor = Color.LightBlue;
            panel3.Controls.Add(pictureBox1);
            panel3.Controls.Add(label1);
            panel3.Controls.Add(label4);
            panel3.Dock = DockStyle.Fill;
            panel3.Location = new Point(3, 3);
            panel3.Name = "panel3";
            panel3.Size = new Size(291, 405);
            panel3.TabIndex = 8;
            // 
            // pictureBox1
            // 
            pictureBox1.Dock = DockStyle.Top;
            pictureBox1.Image = Properties.Resources.Cleanup;
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(291, 257);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 6;
            pictureBox1.TabStop = false;
            // 
            // ConfigurationForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(770, 411);
            Controls.Add(tableLayoutPanel1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ConfigurationForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "LASYS Cleanup";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            panel2.ResumeLayout(false);
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)nudRetention).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Label label1;
        private Label label2;
        private Label label3;
        private TextBox txtPrintJobFolder;
        private Button btnBrowse;
        private Label label4;
        private Label label5;
        private GroupBox groupBox1;
        private Label label6;
        private Label label7;
        private ComboBox cmbRetentionUnit;
        private NumericUpDown nudRetention;
        private Panel panel1;
        private Label label8;
        private Label label9;
        private Label label10;
        private Panel panel2;
        private Label lblScheduleStatus;
        private Label label12;
        private ComboBox cmbRunTime;
        private Label label11;
        private ComboBox cmbFrequency;
        private Button btnCancel;
        private Button btnSaveSettings;
        private Label lblRetentionStatus;
        private TableLayoutPanel tableLayoutPanel1;
        private Panel panel3;
        private PictureBox pictureBox1;
    }
}