namespace LASYS.Cleanup.UI.Views.Configuration
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
            btnClose = new Button();
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
            taskStatusGroupBox = new GroupBox();
            lblLastResultValue = new Label();
            label24 = new Label();
            lblLastRunValue = new Label();
            label22 = new Label();
            lblNextRunValue = new Label();
            label20 = new Label();
            lblScheduledTimeValue = new Label();
            label18 = new Label();
            lblFrequencyValue = new Label();
            label16 = new Label();
            lblTaskStatusValue = new Label();
            label13 = new Label();
            label8 = new Label();
            groupBox1.SuspendLayout();
            panel2.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudRetention).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            taskStatusGroupBox.SuspendLayout();
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
            groupBox1.Controls.Add(btnClose);
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
            groupBox1.Dock = DockStyle.Fill;
            groupBox1.Location = new Point(290, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(472, 405);
            groupBox1.TabIndex = 7;
            groupBox1.TabStop = false;
            // 
            // btnClose
            // 
            btnClose.BackColor = Color.Silver;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.ForeColor = Color.Black;
            btnClose.Image = Properties.Resources.close_16dp_000000_FILL1_wght400_GRAD0_opsz20;
            btnClose.Location = new Point(125, 351);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(90, 39);
            btnClose.TabIndex = 20;
            btnClose.Text = "Close";
            btnClose.TextAlign = ContentAlignment.MiddleRight;
            btnClose.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += btnClose_Click;
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
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.Controls.Add(groupBox1, 1, 0);
            tableLayoutPanel1.Controls.Add(panel3, 0, 0);
            tableLayoutPanel1.Controls.Add(taskStatusGroupBox, 2, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(957, 411);
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
            panel3.Size = new Size(281, 405);
            panel3.TabIndex = 8;
            // 
            // pictureBox1
            // 
            pictureBox1.Dock = DockStyle.Top;
            pictureBox1.Image = Properties.Resources.Cleanup;
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(281, 257);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 6;
            pictureBox1.TabStop = false;
            // 
            // taskStatusGroupBox
            // 
            taskStatusGroupBox.Controls.Add(lblLastResultValue);
            taskStatusGroupBox.Controls.Add(label24);
            taskStatusGroupBox.Controls.Add(lblLastRunValue);
            taskStatusGroupBox.Controls.Add(label22);
            taskStatusGroupBox.Controls.Add(lblNextRunValue);
            taskStatusGroupBox.Controls.Add(label20);
            taskStatusGroupBox.Controls.Add(lblScheduledTimeValue);
            taskStatusGroupBox.Controls.Add(label18);
            taskStatusGroupBox.Controls.Add(lblFrequencyValue);
            taskStatusGroupBox.Controls.Add(label16);
            taskStatusGroupBox.Controls.Add(lblTaskStatusValue);
            taskStatusGroupBox.Controls.Add(label13);
            taskStatusGroupBox.Controls.Add(label8);
            taskStatusGroupBox.Dock = DockStyle.Fill;
            taskStatusGroupBox.Location = new Point(768, 3);
            taskStatusGroupBox.Name = "taskStatusGroupBox";
            taskStatusGroupBox.Size = new Size(186, 405);
            taskStatusGroupBox.TabIndex = 9;
            taskStatusGroupBox.TabStop = false;
            // 
            // lblLastResultValue
            // 
            lblLastResultValue.Font = new Font("Segoe UI", 9.75F, FontStyle.Italic);
            lblLastResultValue.ForeColor = Color.Black;
            lblLastResultValue.Location = new Point(6, 318);
            lblLastResultValue.Name = "lblLastResultValue";
            lblLastResultValue.Size = new Size(171, 79);
            lblLastResultValue.TabIndex = 28;
            lblLastResultValue.Text = "-";
            // 
            // label24
            // 
            label24.AutoSize = true;
            label24.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold);
            label24.Location = new Point(6, 295);
            label24.Name = "label24";
            label24.Size = new Size(76, 17);
            label24.TabIndex = 27;
            label24.Text = "Last Result:";
            // 
            // lblLastRunValue
            // 
            lblLastRunValue.AutoSize = true;
            lblLastRunValue.Font = new Font("Segoe UI", 9.75F, FontStyle.Italic);
            lblLastRunValue.ForeColor = Color.Black;
            lblLastRunValue.Location = new Point(6, 266);
            lblLastRunValue.Name = "lblLastRunValue";
            lblLastRunValue.Size = new Size(13, 17);
            lblLastRunValue.TabIndex = 26;
            lblLastRunValue.Text = "-";
            // 
            // label22
            // 
            label22.AutoSize = true;
            label22.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold);
            label22.Location = new Point(6, 243);
            label22.Name = "label22";
            label22.Size = new Size(63, 17);
            label22.TabIndex = 25;
            label22.Text = "Last Run:";
            // 
            // lblNextRunValue
            // 
            lblNextRunValue.AutoSize = true;
            lblNextRunValue.Font = new Font("Segoe UI", 9.75F, FontStyle.Italic);
            lblNextRunValue.ForeColor = Color.Black;
            lblNextRunValue.Location = new Point(6, 214);
            lblNextRunValue.Name = "lblNextRunValue";
            lblNextRunValue.Size = new Size(13, 17);
            lblNextRunValue.TabIndex = 24;
            lblNextRunValue.Text = "-";
            // 
            // label20
            // 
            label20.AutoSize = true;
            label20.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold);
            label20.Location = new Point(6, 191);
            label20.Name = "label20";
            label20.Size = new Size(68, 17);
            label20.TabIndex = 23;
            label20.Text = "Next Run:";
            // 
            // lblScheduledTimeValue
            // 
            lblScheduledTimeValue.AutoSize = true;
            lblScheduledTimeValue.Font = new Font("Segoe UI", 9.75F, FontStyle.Italic);
            lblScheduledTimeValue.ForeColor = Color.Black;
            lblScheduledTimeValue.Location = new Point(6, 162);
            lblScheduledTimeValue.Name = "lblScheduledTimeValue";
            lblScheduledTimeValue.Size = new Size(13, 17);
            lblScheduledTimeValue.TabIndex = 22;
            lblScheduledTimeValue.Text = "-";
            // 
            // label18
            // 
            label18.AutoSize = true;
            label18.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold);
            label18.Location = new Point(6, 139);
            label18.Name = "label18";
            label18.Size = new Size(106, 17);
            label18.TabIndex = 21;
            label18.Text = "Scheduled Time:";
            // 
            // lblFrequencyValue
            // 
            lblFrequencyValue.AutoSize = true;
            lblFrequencyValue.Font = new Font("Segoe UI", 9.75F, FontStyle.Italic);
            lblFrequencyValue.ForeColor = Color.Black;
            lblFrequencyValue.Location = new Point(6, 110);
            lblFrequencyValue.Name = "lblFrequencyValue";
            lblFrequencyValue.Size = new Size(13, 17);
            lblFrequencyValue.TabIndex = 20;
            lblFrequencyValue.Text = "-";
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold);
            label16.Location = new Point(6, 87);
            label16.Name = "label16";
            label16.Size = new Size(74, 17);
            label16.TabIndex = 19;
            label16.Text = "Frequency:";
            // 
            // lblTaskStatusValue
            // 
            lblTaskStatusValue.AutoSize = true;
            lblTaskStatusValue.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold);
            lblTaskStatusValue.ForeColor = Color.Black;
            lblTaskStatusValue.Location = new Point(61, 58);
            lblTaskStatusValue.Name = "lblTaskStatusValue";
            lblTaskStatusValue.Size = new Size(13, 17);
            lblTaskStatusValue.TabIndex = 18;
            lblTaskStatusValue.Text = "-";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold);
            label13.Location = new Point(6, 58);
            label13.Name = "label13";
            label13.Size = new Size(49, 17);
            label13.TabIndex = 3;
            label13.Text = "Status:";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label8.Location = new Point(6, 19);
            label8.Name = "label8";
            label8.Size = new Size(117, 21);
            label8.TabIndex = 2;
            label8.Text = "Task Scheduler";
            // 
            // ConfigurationForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(957, 411);
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
            taskStatusGroupBox.ResumeLayout(false);
            taskStatusGroupBox.PerformLayout();
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
        private Label label9;
        private Label label10;
        private Panel panel2;
        private Label lblScheduleStatus;
        private Label label12;
        private ComboBox cmbRunTime;
        private Label label11;
        private ComboBox cmbFrequency;
        private Button btnClose;
        private Button btnSaveSettings;
        private Label lblRetentionStatus;
        private TableLayoutPanel tableLayoutPanel1;
        private Panel panel3;
        private PictureBox pictureBox1;
        private GroupBox taskStatusGroupBox;
        private Label label8;
        private Label lblLastResultValue;
        private Label label24;
        private Label lblLastRunValue;
        private Label label22;
        private Label lblNextRunValue;
        private Label label20;
        private Label lblScheduledTimeValue;
        private Label label18;
        private Label lblFrequencyValue;
        private Label label16;
        private Label lblTaskStatusValue;
        private Label label13;
    }
}