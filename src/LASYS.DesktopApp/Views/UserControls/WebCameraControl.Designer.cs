namespace LASYS.DesktopApp.Views.UserControls
{
    partial class WebCameraControl
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
            btnPreview = new Button();
            label2 = new Label();
            cbxCameras = new ComboBox();
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
            pnlHeader.Location = new Point(3, 3);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(827, 48);
            pnlHeader.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.FromArgb(240, 84, 84);
            label1.Location = new Point(12, 8);
            label1.Name = "label1";
            label1.Size = new Size(136, 30);
            label1.TabIndex = 0;
            label1.Text = "Web Camera";
            // 
            // pnlContent
            // 
            pnlContent.Anchor = AnchorStyles.None;
            pnlContent.BackColor = Color.White;
            pnlContent.Controls.Add(picCameraPreview);
            pnlContent.Controls.Add(btnPreview);
            pnlContent.Controls.Add(label2);
            pnlContent.Controls.Add(cbxCameras);
            pnlContent.Location = new Point(160, 78);
            pnlContent.Name = "pnlContent";
            pnlContent.Size = new Size(571, 416);
            pnlContent.TabIndex = 1;
            // 
            // picCameraPreview
            // 
            picCameraPreview.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            picCameraPreview.BackColor = Color.FromArgb(64, 64, 64);
            picCameraPreview.Location = new Point(27, 115);
            picCameraPreview.Name = "picCameraPreview";
            picCameraPreview.Size = new Size(516, 278);
            picCameraPreview.TabIndex = 3;
            picCameraPreview.TabStop = false;
            // 
            // btnPreview
            // 
            btnPreview.Anchor = AnchorStyles.Top;
            btnPreview.BackColor = SystemColors.HotTrack;
            btnPreview.FlatAppearance.BorderColor = SystemColors.HotTrack;
            btnPreview.FlatStyle = FlatStyle.Flat;
            btnPreview.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnPreview.ForeColor = Color.White;
            btnPreview.Location = new Point(433, 55);
            btnPreview.Name = "btnPreview";
            btnPreview.Size = new Size(110, 29);
            btnPreview.TabIndex = 2;
            btnPreview.Text = "Preview";
            btnPreview.UseVisualStyleBackColor = false;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top;
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.ForeColor = Color.Black;
            label2.Location = new Point(27, 18);
            label2.Name = "label2";
            label2.Size = new Size(150, 30);
            label2.TabIndex = 1;
            label2.Text = "Select Camera";
            // 
            // cbxCameras
            // 
            cbxCameras.Anchor = AnchorStyles.Top;
            cbxCameras.DropDownStyle = ComboBoxStyle.DropDownList;
            cbxCameras.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            cbxCameras.FormattingEnabled = true;
            cbxCameras.Items.AddRange(new object[] { "Built-In Camera" });
            cbxCameras.Location = new Point(27, 55);
            cbxCameras.Name = "cbxCameras";
            cbxCameras.Size = new Size(390, 29);
            cbxCameras.TabIndex = 0;
            // 
            // WebCameraControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(pnlContent);
            Controls.Add(pnlHeader);
            Name = "WebCameraControl";
            Size = new Size(833, 519);
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlContent.ResumeLayout(false);
            pnlContent.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picCameraPreview).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlHeader;
        private Label label1;
        private Panel pnlContent;
        private PictureBox picCameraPreview;
        private Button btnPreview;
        private Label label2;
        private ComboBox cbxCameras;
    }
}
