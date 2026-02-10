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
            pnlHeader = new Panel();
            label1 = new Label();
            pnlContent = new Panel();
            btnPrint = new Button();
            pnlHeader.SuspendLayout();
            pnlContent.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlHeader.BackColor = Color.White;
            pnlHeader.Controls.Add(label1);
            pnlHeader.Location = new Point(2, 2);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(860, 48);
            pnlHeader.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.FromArgb(240, 84, 84);
            label1.Location = new Point(12, 8);
            label1.Name = "label1";
            label1.Size = new Size(216, 30);
            label1.TabIndex = 0;
            label1.Text = "Printer Management";
            // 
            // pnlContent
            // 
            pnlContent.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlContent.BackColor = Color.White;
            pnlContent.Controls.Add(btnPrint);
            pnlContent.Location = new Point(2, 53);
            pnlContent.Name = "pnlContent";
            pnlContent.Size = new Size(860, 439);
            pnlContent.TabIndex = 3;
            // 
            // btnPrint
            // 
            btnPrint.Anchor = AnchorStyles.None;
            btnPrint.BackColor = Color.DarkSlateGray;
            btnPrint.FlatAppearance.BorderSize = 0;
            btnPrint.FlatStyle = FlatStyle.Flat;
            btnPrint.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold);
            btnPrint.ForeColor = Color.White;
            btnPrint.Image = Properties.Resources.print_add24;
            btnPrint.Location = new Point(369, 187);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(126, 37);
            btnPrint.TabIndex = 6;
            btnPrint.Text = "Add Printer";
            btnPrint.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnPrint.UseVisualStyleBackColor = false;
            // 
            // PrinterManagementControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(pnlContent);
            Controls.Add(pnlHeader);
            Name = "PrinterManagementControl";
            Size = new Size(864, 495);
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlContent.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlHeader;
        private Label label1;
        private Panel pnlContent;
        private Button btnPrint;
    }
}
