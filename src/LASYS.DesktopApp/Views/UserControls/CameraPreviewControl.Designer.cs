namespace LASYS.DesktopApp.Views.UserControls
{
    partial class CameraPreviewControl
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
            picPreview = new PictureBox();
            lblNoCameraDisplayText = new Label();
            ((System.ComponentModel.ISupportInitialize)picPreview).BeginInit();
            SuspendLayout();
            // 
            // picPreview
            // 
            picPreview.BackColor = SystemColors.ActiveCaptionText;
            picPreview.Dock = DockStyle.Fill;
            picPreview.Location = new Point(0, 0);
            picPreview.Name = "picPreview";
            picPreview.Size = new Size(421, 272);
            picPreview.SizeMode = PictureBoxSizeMode.Zoom;
            picPreview.TabIndex = 0;
            picPreview.TabStop = false;
            // 
            // lblNoCameraDisplayText
            // 
            lblNoCameraDisplayText.Anchor = AnchorStyles.None;
            lblNoCameraDisplayText.AutoSize = true;
            lblNoCameraDisplayText.BackColor = SystemColors.ActiveCaptionText;
            lblNoCameraDisplayText.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblNoCameraDisplayText.ForeColor = Color.White;
            lblNoCameraDisplayText.Location = new Point(40, 122);
            lblNoCameraDisplayText.Name = "lblNoCameraDisplayText";
            lblNoCameraDisplayText.Size = new Size(318, 21);
            lblNoCameraDisplayText.TabIndex = 1;
            lblNoCameraDisplayText.Text = "Connect the camera to view the live preview.";
            // 
            // CameraPreviewControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(lblNoCameraDisplayText);
            Controls.Add(picPreview);
            Name = "CameraPreviewControl";
            Size = new Size(421, 272);
            ((System.ComponentModel.ISupportInitialize)picPreview).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox picPreview;
        private Label lblNoCameraDisplayText;
    }
}
