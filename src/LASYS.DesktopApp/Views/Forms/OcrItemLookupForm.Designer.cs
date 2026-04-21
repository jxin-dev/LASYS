namespace LASYS.DesktopApp.Views.Forms
{
    partial class OcrItemLookupForm
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
            lblHeader = new Label();
            pnlContainer = new Panel();
            btnClose = new Button();
            SuspendLayout();
            // 
            // lblHeader
            // 
            lblHeader.BackColor = Color.FromArgb(15, 127, 102);
            lblHeader.Dock = DockStyle.Top;
            lblHeader.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblHeader.ForeColor = Color.White;
            lblHeader.ImageAlign = ContentAlignment.MiddleLeft;
            lblHeader.Location = new Point(0, 0);
            lblHeader.Name = "lblHeader";
            lblHeader.Size = new Size(574, 44);
            lblHeader.TabIndex = 5;
            lblHeader.Text = "Choose Item";
            lblHeader.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pnlContainer
            // 
            pnlContainer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlContainer.Location = new Point(12, 61);
            pnlContainer.Name = "pnlContainer";
            pnlContainer.Size = new Size(550, 322);
            pnlContainer.TabIndex = 6;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Bottom;
            btnClose.BackColor = Color.FromArgb(0, 140, 125);
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Segoe UI", 12F);
            btnClose.ForeColor = Color.White;
            btnClose.Location = new Point(242, 389);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(91, 37);
            btnClose.TabIndex = 7;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = false;
            // 
            // OcrItemLookupForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 248, 247);
            ClientSize = new Size(574, 435);
            Controls.Add(btnClose);
            Controls.Add(pnlContainer);
            Controls.Add(lblHeader);
            FormBorderStyle = FormBorderStyle.None;
            Name = "OcrItemLookupForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Ocr Item Lookup";
            ResumeLayout(false);
        }

        #endregion

        private Label lblHeader;
        private Panel pnlContainer;
        private Button btnClose;
    }
}