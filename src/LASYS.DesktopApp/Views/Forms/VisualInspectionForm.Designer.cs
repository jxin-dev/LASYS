namespace LASYS.DesktopApp.Views.Forms
{
    partial class VisualInspectionForm
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
            label1 = new Label();
            pnlBody = new Panel();
            flwContent = new FlowLayoutPanel();
            panel14 = new Panel();
            label14 = new Label();
            label15 = new Label();
            panel2 = new Panel();
            label4 = new Label();
            label5 = new Label();
            pnlLabelVisualInspection = new Panel();
            lblVisual = new Label();
            label7 = new Label();
            pnlLabelVerification = new Panel();
            lblVerification = new Label();
            label9 = new Label();
            pnlInspection = new Panel();
            lblInspectionTitle = new Label();
            rdbFailed = new RadioButton();
            rdbPassed = new RadioButton();
            panel1 = new Panel();
            lblSequenceNo = new Label();
            label3 = new Label();
            flowLayoutPanel1 = new FlowLayoutPanel();
            btnProceed = new Button();
            btnBack = new Button();
            pnlBody.SuspendLayout();
            flwContent.SuspendLayout();
            panel14.SuspendLayout();
            panel2.SuspendLayout();
            pnlLabelVisualInspection.SuspendLayout();
            pnlLabelVerification.SuspendLayout();
            pnlInspection.SuspendLayout();
            panel1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.BackColor = Color.FromArgb(255, 204, 0);
            label1.Dock = DockStyle.Top;
            label1.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.Black;
            label1.Image = Properties.Resources.frame_inspect_24;
            label1.ImageAlign = ContentAlignment.MiddleLeft;
            label1.Location = new Point(0, 0);
            label1.Name = "label1";
            label1.Size = new Size(438, 44);
            label1.TabIndex = 4;
            label1.Text = "     Visual Inspection";
            label1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlBody
            // 
            pnlBody.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlBody.Controls.Add(flwContent);
            pnlBody.Controls.Add(panel1);
            pnlBody.Controls.Add(flowLayoutPanel1);
            pnlBody.Location = new Point(12, 58);
            pnlBody.Name = "pnlBody";
            pnlBody.Size = new Size(414, 285);
            pnlBody.TabIndex = 5;
            // 
            // flwContent
            // 
            flwContent.BackColor = Color.White;
            flwContent.Controls.Add(panel14);
            flwContent.Controls.Add(panel2);
            flwContent.Controls.Add(pnlLabelVisualInspection);
            flwContent.Controls.Add(pnlLabelVerification);
            flwContent.Controls.Add(pnlInspection);
            flwContent.Dock = DockStyle.Fill;
            flwContent.Location = new Point(0, 34);
            flwContent.Name = "flwContent";
            flwContent.Size = new Size(414, 200);
            flwContent.TabIndex = 2;
            // 
            // panel14
            // 
            panel14.BackColor = Color.White;
            panel14.Controls.Add(label14);
            panel14.Controls.Add(label15);
            panel14.Dock = DockStyle.Top;
            panel14.Location = new Point(3, 3);
            panel14.Name = "panel14";
            panel14.Size = new Size(389, 23);
            panel14.TabIndex = 2;
            // 
            // label14
            // 
            label14.Dock = DockStyle.Fill;
            label14.Font = new Font("Segoe UI Black", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label14.ForeColor = Color.DarkGreen;
            label14.Location = new Point(302, 0);
            label14.Name = "label14";
            label14.Size = new Size(87, 23);
            label14.TabIndex = 1;
            label14.Text = "PASSED";
            label14.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label15
            // 
            label15.Dock = DockStyle.Left;
            label15.Font = new Font("Segoe UI Black", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label15.Location = new Point(0, 0);
            label15.Name = "label15";
            label15.Size = new Size(302, 23);
            label15.TabIndex = 0;
            label15.Text = "Barcode Validation - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -";
            label15.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panel2
            // 
            panel2.BackColor = Color.White;
            panel2.Controls.Add(label4);
            panel2.Controls.Add(label5);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(3, 32);
            panel2.Name = "panel2";
            panel2.Size = new Size(389, 23);
            panel2.TabIndex = 3;
            // 
            // label4
            // 
            label4.Dock = DockStyle.Fill;
            label4.Font = new Font("Segoe UI Black", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label4.ForeColor = Color.DarkGreen;
            label4.Location = new Point(302, 0);
            label4.Name = "label4";
            label4.Size = new Size(87, 23);
            label4.TabIndex = 1;
            label4.Text = "PASSED";
            label4.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            label5.Dock = DockStyle.Left;
            label5.Font = new Font("Segoe UI Black", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label5.Location = new Point(0, 0);
            label5.Name = "label5";
            label5.Size = new Size(302, 23);
            label5.TabIndex = 0;
            label5.Text = "OCR Validation   - - - - - - - - - - - - - - - - - - - - - - - - - - - - -";
            label5.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlLabelVisualInspection
            // 
            pnlLabelVisualInspection.BackColor = Color.White;
            pnlLabelVisualInspection.Controls.Add(lblVisual);
            pnlLabelVisualInspection.Controls.Add(label7);
            pnlLabelVisualInspection.Dock = DockStyle.Top;
            pnlLabelVisualInspection.Location = new Point(3, 61);
            pnlLabelVisualInspection.Name = "pnlLabelVisualInspection";
            pnlLabelVisualInspection.Size = new Size(389, 23);
            pnlLabelVisualInspection.TabIndex = 4;
            pnlLabelVisualInspection.Visible = false;
            // 
            // lblVisual
            // 
            lblVisual.Dock = DockStyle.Fill;
            lblVisual.Font = new Font("Segoe UI Black", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblVisual.ForeColor = Color.DarkGreen;
            lblVisual.Location = new Point(302, 0);
            lblVisual.Name = "lblVisual";
            lblVisual.Size = new Size(87, 23);
            lblVisual.TabIndex = 1;
            lblVisual.Text = "PASSED";
            lblVisual.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label7
            // 
            label7.Dock = DockStyle.Left;
            label7.Font = new Font("Segoe UI Black", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label7.Location = new Point(0, 0);
            label7.Name = "label7";
            label7.Size = new Size(302, 23);
            label7.TabIndex = 0;
            label7.Text = "Label Visual Inspection - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -";
            label7.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlLabelVerification
            // 
            pnlLabelVerification.BackColor = Color.White;
            pnlLabelVerification.Controls.Add(lblVerification);
            pnlLabelVerification.Controls.Add(label9);
            pnlLabelVerification.Dock = DockStyle.Top;
            pnlLabelVerification.Location = new Point(3, 90);
            pnlLabelVerification.Name = "pnlLabelVerification";
            pnlLabelVerification.Size = new Size(389, 23);
            pnlLabelVerification.TabIndex = 5;
            pnlLabelVerification.Visible = false;
            // 
            // lblVerification
            // 
            lblVerification.Dock = DockStyle.Fill;
            lblVerification.Font = new Font("Segoe UI Black", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblVerification.ForeColor = Color.DarkGreen;
            lblVerification.Location = new Point(302, 0);
            lblVerification.Name = "lblVerification";
            lblVerification.Size = new Size(87, 23);
            lblVerification.TabIndex = 1;
            lblVerification.Text = "PASSED";
            lblVerification.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label9
            // 
            label9.Dock = DockStyle.Left;
            label9.Font = new Font("Segoe UI Black", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label9.Location = new Point(0, 0);
            label9.Name = "label9";
            label9.Size = new Size(302, 23);
            label9.TabIndex = 0;
            label9.Text = "Label Verification   - - - - - - - - - - - - - - - - - - - - - - - - - - - - -";
            label9.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlInspection
            // 
            pnlInspection.BackColor = Color.White;
            pnlInspection.BorderStyle = BorderStyle.FixedSingle;
            pnlInspection.Controls.Add(lblInspectionTitle);
            pnlInspection.Controls.Add(rdbFailed);
            pnlInspection.Controls.Add(rdbPassed);
            pnlInspection.Location = new Point(3, 119);
            pnlInspection.Name = "pnlInspection";
            pnlInspection.Size = new Size(407, 74);
            pnlInspection.TabIndex = 6;
            // 
            // lblInspectionTitle
            // 
            lblInspectionTitle.BackColor = Color.Black;
            lblInspectionTitle.Dock = DockStyle.Top;
            lblInspectionTitle.ForeColor = Color.White;
            lblInspectionTitle.Location = new Point(0, 0);
            lblInspectionTitle.Name = "lblInspectionTitle";
            lblInspectionTitle.Size = new Size(405, 23);
            lblInspectionTitle.TabIndex = 4;
            lblInspectionTitle.Text = "Please perform Visual Inspection";
            lblInspectionTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // rdbFailed
            // 
            rdbFailed.AutoSize = true;
            rdbFailed.Font = new Font("Segoe UI Black", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            rdbFailed.ForeColor = Color.Firebrick;
            rdbFailed.Location = new Point(204, 35);
            rdbFailed.Name = "rdbFailed";
            rdbFailed.Size = new Size(75, 25);
            rdbFailed.TabIndex = 3;
            rdbFailed.TabStop = true;
            rdbFailed.Text = "Failed";
            rdbFailed.UseVisualStyleBackColor = true;
            // 
            // rdbPassed
            // 
            rdbPassed.AutoSize = true;
            rdbPassed.Font = new Font("Segoe UI Black", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            rdbPassed.ForeColor = Color.DarkGreen;
            rdbPassed.Location = new Point(116, 35);
            rdbPassed.Name = "rdbPassed";
            rdbPassed.Size = new Size(82, 25);
            rdbPassed.TabIndex = 2;
            rdbPassed.TabStop = true;
            rdbPassed.Text = "Passed";
            rdbPassed.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            panel1.BackColor = Color.White;
            panel1.Controls.Add(lblSequenceNo);
            panel1.Controls.Add(label3);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(414, 34);
            panel1.TabIndex = 1;
            // 
            // lblSequenceNo
            // 
            lblSequenceNo.Dock = DockStyle.Fill;
            lblSequenceNo.Font = new Font("Century Gothic", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblSequenceNo.Location = new Point(316, 0);
            lblSequenceNo.Name = "lblSequenceNo";
            lblSequenceNo.Size = new Size(98, 34);
            lblSequenceNo.TabIndex = 1;
            lblSequenceNo.Text = "000001";
            lblSequenceNo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            label3.Dock = DockStyle.Left;
            label3.Font = new Font("Segoe UI Black", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.Location = new Point(0, 0);
            label3.Name = "label3";
            label3.Size = new Size(316, 34);
            label3.TabIndex = 0;
            label3.Text = "Sequence Number - - - - - - - - - - - - - -";
            label3.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(btnProceed);
            flowLayoutPanel1.Controls.Add(btnBack);
            flowLayoutPanel1.Dock = DockStyle.Bottom;
            flowLayoutPanel1.FlowDirection = FlowDirection.RightToLeft;
            flowLayoutPanel1.Location = new Point(0, 234);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(414, 51);
            flowLayoutPanel1.TabIndex = 0;
            // 
            // btnProceed
            // 
            btnProceed.BackColor = SystemColors.HotTrack;
            btnProceed.FlatAppearance.BorderSize = 0;
            btnProceed.FlatStyle = FlatStyle.Flat;
            btnProceed.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold);
            btnProceed.ForeColor = Color.White;
            btnProceed.Image = Properties.Resources.arrow_right_alt_24;
            btnProceed.Location = new Point(312, 3);
            btnProceed.Name = "btnProceed";
            btnProceed.Size = new Size(99, 44);
            btnProceed.TabIndex = 2;
            btnProceed.Text = "Next";
            btnProceed.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnProceed.UseVisualStyleBackColor = false;
            // 
            // btnBack
            // 
            btnBack.AutoSize = true;
            btnBack.BackColor = Color.DarkGray;
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.FlatStyle = FlatStyle.Flat;
            btnBack.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold);
            btnBack.Image = Properties.Resources.arrow_left_alt_24;
            btnBack.Location = new Point(209, 3);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(97, 44);
            btnBack.TabIndex = 3;
            btnBack.Text = "Back";
            btnBack.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnBack.UseVisualStyleBackColor = false;
            // 
            // VisualInspectionForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(438, 355);
            Controls.Add(pnlBody);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.None;
            Name = "VisualInspectionForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Visual Inspection";
            pnlBody.ResumeLayout(false);
            flwContent.ResumeLayout(false);
            panel14.ResumeLayout(false);
            panel2.ResumeLayout(false);
            pnlLabelVisualInspection.ResumeLayout(false);
            pnlLabelVerification.ResumeLayout(false);
            pnlInspection.ResumeLayout(false);
            pnlInspection.PerformLayout();
            panel1.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Label label1;
        private Panel pnlBody;
        private FlowLayoutPanel flowLayoutPanel1;
        private Button btnProceed;
        private Button btnBack;
        private Panel panel1;
        private Label lblSequenceNo;
        private Label label3;
        private FlowLayoutPanel flwContent;
        private Panel panel14;
        private Label label14;
        private Label label15;
        private Panel panel2;
        private Label label4;
        private Label label5;
        private Panel pnlLabelVisualInspection;
        private Label lblVisual;
        private Label label7;
        private Panel pnlLabelVerification;
        private Label lblVerification;
        private Label label9;
        private Panel pnlInspection;
        private RadioButton rdbFailed;
        private RadioButton rdbPassed;
        private Label lblInspectionTitle;
    }
}