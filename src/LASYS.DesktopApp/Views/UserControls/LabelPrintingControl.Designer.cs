namespace LASYS.DesktopApp.Views.UserControls
{
    partial class LabelPrintingControl
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
            pnlLoadingContainer = new Panel();
            btnPrint = new Button();
            button1 = new Button();
            groupBox4 = new GroupBox();
            lblLabelSample = new Label();
            label30 = new Label();
            lblTotalFailed = new Label();
            label28 = new Label();
            label17 = new Label();
            lblTotalQuantity = new Label();
            lblTotalPassed = new Label();
            label26 = new Label();
            label22 = new Label();
            lblRemaining = new Label();
            lblTotalPrinted = new Label();
            label24 = new Label();
            groupBox3 = new GroupBox();
            lblSetNumber = new Label();
            label15 = new Label();
            lblBatchNumber = new Label();
            cbEndOfBatch = new CheckBox();
            label13 = new Label();
            groupBox2 = new GroupBox();
            nudQuantity = new NumericUpDown();
            lblStartSequence = new Label();
            label19 = new Label();
            label21 = new Label();
            groupBox1 = new GroupBox();
            lblLabelFile = new Label();
            label9 = new Label();
            lblLotNo = new Label();
            label11 = new Label();
            lblExpiryDate = new Label();
            label7 = new Label();
            lblItemCode = new Label();
            label5 = new Label();
            lblInstructionCode = new Label();
            label2 = new Label();
            pnlHeader = new Panel();
            btnBack = new Button();
            label1 = new Label();
            pnlContent.SuspendLayout();
            groupBox4.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudQuantity).BeginInit();
            groupBox1.SuspendLayout();
            pnlHeader.SuspendLayout();
            SuspendLayout();
            // 
            // pnlContent
            // 
            pnlContent.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlContent.BackColor = Color.White;
            pnlContent.Controls.Add(pnlLoadingContainer);
            pnlContent.Controls.Add(btnPrint);
            pnlContent.Controls.Add(button1);
            pnlContent.Controls.Add(groupBox4);
            pnlContent.Controls.Add(groupBox3);
            pnlContent.Controls.Add(groupBox2);
            pnlContent.Controls.Add(groupBox1);
            pnlContent.Location = new Point(3, 54);
            pnlContent.Name = "pnlContent";
            pnlContent.Size = new Size(1235, 515);
            pnlContent.TabIndex = 4;
            // 
            // pnlLoadingContainer
            // 
            pnlLoadingContainer.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlLoadingContainer.Location = new Point(3, 452);
            pnlLoadingContainer.Name = "pnlLoadingContainer";
            pnlLoadingContainer.Size = new Size(1229, 60);
            pnlLoadingContainer.TabIndex = 6;
            // 
            // btnPrint
            // 
            btnPrint.Anchor = AnchorStyles.None;
            btnPrint.BackColor = Color.Crimson;
            btnPrint.FlatAppearance.BorderSize = 0;
            btnPrint.FlatStyle = FlatStyle.Flat;
            btnPrint.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold);
            btnPrint.ForeColor = Color.White;
            btnPrint.Image = Properties.Resources.stopbatch24;
            btnPrint.Location = new Point(993, 312);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(122, 37);
            btnPrint.TabIndex = 5;
            btnPrint.Text = "Stop Print";
            btnPrint.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnPrint.UseVisualStyleBackColor = false;
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.None;
            button1.BackColor = Color.DarkOrange;
            button1.FlatAppearance.BorderSize = 0;
            button1.FlatStyle = FlatStyle.Flat;
            button1.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold);
            button1.Image = Properties.Resources.pause24;
            button1.Location = new Point(893, 312);
            button1.Name = "button1";
            button1.Size = new Size(94, 37);
            button1.TabIndex = 4;
            button1.Text = "Pause";
            button1.TextImageRelation = TextImageRelation.ImageBeforeText;
            button1.UseVisualStyleBackColor = false;
            // 
            // groupBox4
            // 
            groupBox4.Anchor = AnchorStyles.None;
            groupBox4.Controls.Add(lblLabelSample);
            groupBox4.Controls.Add(label30);
            groupBox4.Controls.Add(lblTotalFailed);
            groupBox4.Controls.Add(label28);
            groupBox4.Controls.Add(label17);
            groupBox4.Controls.Add(lblTotalQuantity);
            groupBox4.Controls.Add(lblTotalPassed);
            groupBox4.Controls.Add(label26);
            groupBox4.Controls.Add(label22);
            groupBox4.Controls.Add(lblRemaining);
            groupBox4.Controls.Add(lblTotalPrinted);
            groupBox4.Controls.Add(label24);
            groupBox4.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            groupBox4.Location = new Point(582, 156);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(533, 150);
            groupBox4.TabIndex = 3;
            groupBox4.TabStop = false;
            groupBox4.Text = "Printing Result Information";
            // 
            // lblLabelSample
            // 
            lblLabelSample.AutoSize = true;
            lblLabelSample.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold | FontStyle.Underline);
            lblLabelSample.Location = new Point(139, 93);
            lblLabelSample.Name = "lblLabelSample";
            lblLabelSample.Size = new Size(17, 20);
            lblLabelSample.TabIndex = 21;
            lblLabelSample.Text = "0";
            // 
            // label30
            // 
            label30.AutoSize = true;
            label30.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label30.Location = new Point(19, 93);
            label30.Name = "label30";
            label30.Size = new Size(94, 17);
            label30.TabIndex = 20;
            label30.Text = "Label Sample:";
            // 
            // lblTotalFailed
            // 
            lblTotalFailed.AutoSize = true;
            lblTotalFailed.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold | FontStyle.Underline);
            lblTotalFailed.Location = new Point(371, 93);
            lblTotalFailed.Name = "lblTotalFailed";
            lblTotalFailed.Size = new Size(17, 20);
            lblTotalFailed.TabIndex = 19;
            lblTotalFailed.Text = "0";
            // 
            // label28
            // 
            label28.AutoSize = true;
            label28.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label28.Location = new Point(19, 51);
            label28.Name = "label28";
            label28.Size = new Size(109, 17);
            label28.TabIndex = 10;
            label28.Text = "Target Quantity:";
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label17.Location = new Point(251, 93);
            label17.Name = "label17";
            label17.Size = new Size(84, 17);
            label17.TabIndex = 18;
            label17.Text = "Total Failed:";
            // 
            // lblTotalQuantity
            // 
            lblTotalQuantity.AutoSize = true;
            lblTotalQuantity.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold | FontStyle.Underline);
            lblTotalQuantity.Location = new Point(139, 51);
            lblTotalQuantity.Name = "lblTotalQuantity";
            lblTotalQuantity.Size = new Size(17, 20);
            lblTotalQuantity.TabIndex = 11;
            lblTotalQuantity.Text = "0";
            // 
            // lblTotalPassed
            // 
            lblTotalPassed.AutoSize = true;
            lblTotalPassed.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold | FontStyle.Underline);
            lblTotalPassed.Location = new Point(371, 72);
            lblTotalPassed.Name = "lblTotalPassed";
            lblTotalPassed.Size = new Size(17, 20);
            lblTotalPassed.TabIndex = 17;
            lblTotalPassed.Text = "0";
            // 
            // label26
            // 
            label26.AutoSize = true;
            label26.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label26.Location = new Point(19, 72);
            label26.Name = "label26";
            label26.Size = new Size(78, 17);
            label26.TabIndex = 12;
            label26.Text = "Remaining:";
            // 
            // label22
            // 
            label22.AutoSize = true;
            label22.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label22.Location = new Point(251, 72);
            label22.Name = "label22";
            label22.Size = new Size(89, 17);
            label22.TabIndex = 16;
            label22.Text = "Total Passed:";
            // 
            // lblRemaining
            // 
            lblRemaining.AutoSize = true;
            lblRemaining.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold | FontStyle.Underline);
            lblRemaining.Location = new Point(139, 72);
            lblRemaining.Name = "lblRemaining";
            lblRemaining.Size = new Size(17, 20);
            lblRemaining.TabIndex = 13;
            lblRemaining.Text = "0";
            // 
            // lblTotalPrinted
            // 
            lblTotalPrinted.AutoSize = true;
            lblTotalPrinted.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold | FontStyle.Underline);
            lblTotalPrinted.Location = new Point(371, 51);
            lblTotalPrinted.Name = "lblTotalPrinted";
            lblTotalPrinted.Size = new Size(17, 20);
            lblTotalPrinted.TabIndex = 15;
            lblTotalPrinted.Text = "0";
            // 
            // label24
            // 
            label24.AutoSize = true;
            label24.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label24.Location = new Point(251, 51);
            label24.Name = "label24";
            label24.Size = new Size(92, 17);
            label24.TabIndex = 14;
            label24.Text = "Total Printed:";
            // 
            // groupBox3
            // 
            groupBox3.Anchor = AnchorStyles.None;
            groupBox3.Controls.Add(lblSetNumber);
            groupBox3.Controls.Add(label15);
            groupBox3.Controls.Add(lblBatchNumber);
            groupBox3.Controls.Add(cbEndOfBatch);
            groupBox3.Controls.Add(label13);
            groupBox3.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            groupBox3.Location = new Point(582, 87);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(533, 68);
            groupBox3.TabIndex = 2;
            groupBox3.TabStop = false;
            groupBox3.Text = "Batch Information";
            // 
            // lblSetNumber
            // 
            lblSetNumber.AutoSize = true;
            lblSetNumber.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold | FontStyle.Underline, GraphicsUnit.Point, 0);
            lblSetNumber.Location = new Point(470, 30);
            lblSetNumber.Name = "lblSetNumber";
            lblSetNumber.Size = new Size(17, 20);
            lblSetNumber.TabIndex = 8;
            lblSetNumber.Text = "0";
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label15.Location = new Point(350, 30);
            label15.Name = "label15";
            label15.Size = new Size(86, 17);
            label15.TabIndex = 7;
            label15.Text = "Set Number:";
            // 
            // lblBatchNumber
            // 
            lblBatchNumber.AutoSize = true;
            lblBatchNumber.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold | FontStyle.Underline, GraphicsUnit.Point, 0);
            lblBatchNumber.Location = new Point(295, 30);
            lblBatchNumber.Name = "lblBatchNumber";
            lblBatchNumber.Size = new Size(17, 20);
            lblBatchNumber.TabIndex = 6;
            lblBatchNumber.Text = "0";
            // 
            // cbEndOfBatch
            // 
            cbEndOfBatch.AutoSize = true;
            cbEndOfBatch.CheckAlign = ContentAlignment.MiddleRight;
            cbEndOfBatch.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            cbEndOfBatch.Location = new Point(19, 29);
            cbEndOfBatch.Name = "cbEndOfBatch";
            cbEndOfBatch.Size = new Size(133, 21);
            cbEndOfBatch.TabIndex = 3;
            cbEndOfBatch.Text = "End of Batch:      ";
            cbEndOfBatch.UseVisualStyleBackColor = true;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label13.Location = new Point(175, 30);
            label13.Name = "label13";
            label13.Size = new Size(101, 17);
            label13.TabIndex = 5;
            label13.Text = "Batch Number:";
            // 
            // groupBox2
            // 
            groupBox2.Anchor = AnchorStyles.None;
            groupBox2.Controls.Add(nudQuantity);
            groupBox2.Controls.Add(lblStartSequence);
            groupBox2.Controls.Add(label19);
            groupBox2.Controls.Add(label21);
            groupBox2.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            groupBox2.Location = new Point(144, 238);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(432, 68);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Barcode Label";
            // 
            // nudQuantity
            // 
            nudQuantity.Location = new Point(139, 25);
            nudQuantity.Name = "nudQuantity";
            nudQuantity.ReadOnly = true;
            nudQuantity.Size = new Size(75, 27);
            nudQuantity.TabIndex = 4;
            nudQuantity.TextAlign = HorizontalAlignment.Right;
            // 
            // lblStartSequence
            // 
            lblStartSequence.AutoSize = true;
            lblStartSequence.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold | FontStyle.Underline, GraphicsUnit.Point, 0);
            lblStartSequence.Location = new Point(360, 29);
            lblStartSequence.Name = "lblStartSequence";
            lblStartSequence.Size = new Size(17, 20);
            lblStartSequence.TabIndex = 3;
            lblStartSequence.Text = "0";
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label19.Location = new Point(240, 29);
            label19.Name = "label19";
            label19.Size = new Size(103, 17);
            label19.TabIndex = 2;
            label19.Text = "Start Sequence:";
            // 
            // label21
            // 
            label21.AutoSize = true;
            label21.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label21.Location = new Point(19, 29);
            label21.Name = "label21";
            label21.Size = new Size(66, 17);
            label21.TabIndex = 0;
            label21.Text = "Quantity:";
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.None;
            groupBox1.Controls.Add(lblLabelFile);
            groupBox1.Controls.Add(label9);
            groupBox1.Controls.Add(lblLotNo);
            groupBox1.Controls.Add(label11);
            groupBox1.Controls.Add(lblExpiryDate);
            groupBox1.Controls.Add(label7);
            groupBox1.Controls.Add(lblItemCode);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(lblInstructionCode);
            groupBox1.Controls.Add(label2);
            groupBox1.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            groupBox1.Location = new Point(144, 87);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(432, 150);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Label Instruction";
            // 
            // lblLabelFile
            // 
            lblLabelFile.AutoSize = true;
            lblLabelFile.Font = new Font("Segoe UI", 9.75F, FontStyle.Underline, GraphicsUnit.Point, 0);
            lblLabelFile.Location = new Point(139, 113);
            lblLabelFile.Name = "lblLabelFile";
            lblLabelFile.Size = new Size(95, 17);
            lblLabelFile.TabIndex = 9;
            lblLabelFile.Text = "Not Applicable";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label9.Location = new Point(19, 113);
            label9.Name = "label9";
            label9.Size = new Size(71, 17);
            label9.TabIndex = 8;
            label9.Text = "Label File:";
            // 
            // lblLotNo
            // 
            lblLotNo.AutoSize = true;
            lblLotNo.Font = new Font("Segoe UI", 9.75F, FontStyle.Underline, GraphicsUnit.Point, 0);
            lblLotNo.Location = new Point(139, 92);
            lblLotNo.Name = "lblLotNo";
            lblLotNo.Size = new Size(95, 17);
            lblLotNo.TabIndex = 7;
            lblLotNo.Text = "Not Applicable";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label11.Location = new Point(19, 92);
            label11.Name = "label11";
            label11.Size = new Size(54, 17);
            label11.TabIndex = 6;
            label11.Text = "Lot No:";
            // 
            // lblExpiryDate
            // 
            lblExpiryDate.AutoSize = true;
            lblExpiryDate.Font = new Font("Segoe UI", 9.75F, FontStyle.Underline, GraphicsUnit.Point, 0);
            lblExpiryDate.Location = new Point(139, 71);
            lblExpiryDate.Name = "lblExpiryDate";
            lblExpiryDate.Size = new Size(95, 17);
            lblExpiryDate.TabIndex = 5;
            lblExpiryDate.Text = "Not Applicable";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label7.Location = new Point(19, 71);
            label7.Name = "label7";
            label7.Size = new Size(84, 17);
            label7.TabIndex = 4;
            label7.Text = "Expiry Date:";
            // 
            // lblItemCode
            // 
            lblItemCode.AutoSize = true;
            lblItemCode.Font = new Font("Segoe UI", 9.75F, FontStyle.Underline, GraphicsUnit.Point, 0);
            lblItemCode.Location = new Point(139, 50);
            lblItemCode.Name = "lblItemCode";
            lblItemCode.Size = new Size(95, 17);
            lblItemCode.TabIndex = 3;
            lblItemCode.Text = "Not Applicable";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label5.Location = new Point(19, 50);
            label5.Name = "label5";
            label5.Size = new Size(75, 17);
            label5.TabIndex = 2;
            label5.Text = "Item Code:";
            // 
            // lblInstructionCode
            // 
            lblInstructionCode.AutoSize = true;
            lblInstructionCode.Font = new Font("Segoe UI", 9.75F, FontStyle.Underline, GraphicsUnit.Point, 0);
            lblInstructionCode.Location = new Point(139, 29);
            lblInstructionCode.Name = "lblInstructionCode";
            lblInstructionCode.Size = new Size(95, 17);
            lblInstructionCode.TabIndex = 1;
            lblInstructionCode.Text = "Not Applicable";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(19, 29);
            label2.Name = "label2";
            label2.Size = new Size(114, 17);
            label2.TabIndex = 0;
            label2.Text = "Instruction Code:";
            // 
            // pnlHeader
            // 
            pnlHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlHeader.BackColor = Color.White;
            pnlHeader.Controls.Add(btnBack);
            pnlHeader.Controls.Add(label1);
            pnlHeader.Location = new Point(3, 3);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(1235, 48);
            pnlHeader.TabIndex = 3;
            // 
            // btnBack
            // 
            btnBack.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBack.BackColor = Color.LightGray;
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.FlatStyle = FlatStyle.Flat;
            btnBack.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold);
            btnBack.ForeColor = Color.Black;
            btnBack.Image = Properties.Resources.return24;
            btnBack.ImageAlign = ContentAlignment.MiddleLeft;
            btnBack.Location = new Point(1155, 7);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(75, 33);
            btnBack.TabIndex = 1;
            btnBack.Text = "Back";
            btnBack.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnBack.UseVisualStyleBackColor = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.FromArgb(240, 84, 84);
            label1.Location = new Point(12, 8);
            label1.Name = "label1";
            label1.Size = new Size(151, 30);
            label1.TabIndex = 0;
            label1.Text = "Label Printing";
            // 
            // LabelPrintingControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(pnlContent);
            Controls.Add(pnlHeader);
            Name = "LabelPrintingControl";
            Size = new Size(1242, 573);
            pnlContent.ResumeLayout(false);
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudQuantity).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlContent;
        private Panel pnlHeader;
        private Label label1;
        private Button btnBack;
        private GroupBox groupBox1;
        private Label lblInstructionCode;
        private Label label2;
        private Label lblLabelFile;
        private Label label9;
        private Label lblLotNo;
        private Label label11;
        private Label lblExpiryDate;
        private Label label7;
        private Label lblItemCode;
        private Label label5;
        private GroupBox groupBox2;
        private Label lblStartSequence;
        private Label label19;
        private Label label21;
        private NumericUpDown nudQuantity;
        private GroupBox groupBox3;
        private Label lblSetNumber;
        private Label label15;
        private Label lblBatchNumber;
        private CheckBox cbEndOfBatch;
        private Label label13;
        private GroupBox groupBox4;
        private Label lblLabelSample;
        private Label label30;
        private Label lblTotalFailed;
        private Label label28;
        private Label label17;
        private Label lblTotalQuantity;
        private Label lblTotalPassed;
        private Label label26;
        private Label label22;
        private Label lblRemaining;
        private Label lblTotalPrinted;
        private Label label24;
        private Button button1;
        private Button btnPrint;
        private Panel pnlLoadingContainer;
    }
}
