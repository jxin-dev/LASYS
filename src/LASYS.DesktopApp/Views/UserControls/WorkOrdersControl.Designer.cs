namespace LASYS.DesktopApp.Views.UserControls
{
    partial class WorkOrdersControl
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
            pnlHeader = new Panel();
            label1 = new Label();
            panel1 = new Panel();
            label2 = new Label();
            txtLabelInstructionBarcode = new TextBox();
            pnlContent = new Panel();
            pnlHeader.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlHeader.BackColor = Color.White;
            pnlHeader.Controls.Add(label1);
            pnlHeader.Location = new Point(3, 3);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(745, 48);
            pnlHeader.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.FromArgb(240, 84, 84);
            label1.Location = new Point(12, 8);
            label1.Name = "label1";
            label1.Size = new Size(137, 30);
            label1.TabIndex = 0;
            label1.Text = "Work Orders";
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel1.BackColor = Color.White;
            panel1.Controls.Add(label2);
            panel1.Controls.Add(txtLabelInstructionBarcode);
            panel1.Controls.Add(pnlContent);
            panel1.Location = new Point(3, 57);
            panel1.Name = "panel1";
            panel1.Size = new Size(745, 348);
            panel1.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.ForeColor = Color.FromArgb(0, 110, 100);
            label2.Location = new Point(12, 17);
            label2.Name = "label2";
            label2.Size = new Size(134, 17);
            label2.TabIndex = 3;
            label2.Text = "Instruction Barcode :";
            // 
            // txtLabelInstructionBarcode
            // 
            txtLabelInstructionBarcode.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtLabelInstructionBarcode.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtLabelInstructionBarcode.Location = new Point(152, 14);
            txtLabelInstructionBarcode.Name = "txtLabelInstructionBarcode";
            txtLabelInstructionBarcode.PlaceholderText = "Enter or scan the label instruction barcode...";
            txtLabelInstructionBarcode.Size = new Size(581, 25);
            txtLabelInstructionBarcode.TabIndex = 2;
            // 
            // pnlContent
            // 
            pnlContent.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlContent.Location = new Point(12, 45);
            pnlContent.Name = "pnlContent";
            pnlContent.Size = new Size(721, 270);
            pnlContent.TabIndex = 1;
            // 
            // WorkOrdersControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 248, 247);
            Controls.Add(panel1);
            Controls.Add(pnlHeader);
            Name = "WorkOrdersControl";
            Size = new Size(751, 408);
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlHeader;
        private Label label1;
        private Panel panel1;
        private Panel pnlContent;
        private Label label2;
        private TextBox textBox1;
        private TextBox txtLabelInstructionBarcode;
    }
}
