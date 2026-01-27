namespace LASYS.DesktopApp.Views.UserControls
{
    partial class OCRCalibrationControl
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
            picCameraPreview = new PictureBox();
            pnlHeader.SuspendLayout();
            pnlContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picCameraPreview).BeginInit();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlHeader.BackColor = Color.White;
            pnlHeader.Controls.Add(label1);
            pnlHeader.Location = new Point(4, 3);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(886, 48);
            pnlHeader.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.FromArgb(240, 84, 84);
            label1.Location = new Point(12, 8);
            label1.Name = "label1";
            label1.Size = new Size(170, 30);
            label1.TabIndex = 0;
            label1.Text = "OCR Calibration";
            // 
            // pnlContent
            // 
            pnlContent.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlContent.BackColor = Color.White;
            pnlContent.Controls.Add(picCameraPreview);
            pnlContent.Location = new Point(4, 55);
            pnlContent.Name = "pnlContent";
            pnlContent.Size = new Size(886, 426);
            pnlContent.TabIndex = 2;
            // 
            // picCameraPreview
            // 
            picCameraPreview.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            picCameraPreview.BackColor = Color.FromArgb(64, 64, 64);
            picCameraPreview.Location = new Point(3, 3);
            picCameraPreview.Name = "picCameraPreview";
            picCameraPreview.Size = new Size(880, 391);
            picCameraPreview.SizeMode = PictureBoxSizeMode.StretchImage;
            picCameraPreview.TabIndex = 4;
            picCameraPreview.TabStop = false;
            // 
            // OCRCalibrationControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(pnlContent);
            Controls.Add(pnlHeader);
            Name = "OCRCalibrationControl";
            Size = new Size(893, 484);
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlContent.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picCameraPreview).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlHeader;
        private Label label1;
        private PictureBox picCameraPreview;
        private Panel pnlContent;
    }
}
