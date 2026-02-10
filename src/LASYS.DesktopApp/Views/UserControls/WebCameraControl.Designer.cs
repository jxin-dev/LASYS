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
            label4 = new Label();
            label3 = new Label();
            cbxResolutions = new ComboBox();
            btnSave = new Button();
            picCameraPreview = new PictureBox();
            label2 = new Label();
            cbxCameras = new ComboBox();
            label5 = new Label();
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
            pnlContent.Controls.Add(label5);
            pnlContent.Controls.Add(label4);
            pnlContent.Controls.Add(label3);
            pnlContent.Controls.Add(cbxResolutions);
            pnlContent.Controls.Add(btnSave);
            pnlContent.Controls.Add(picCameraPreview);
            pnlContent.Controls.Add(label2);
            pnlContent.Controls.Add(cbxCameras);
            pnlContent.Location = new Point(50, 57);
            pnlContent.Name = "pnlContent";
            pnlContent.Size = new Size(738, 470);
            pnlContent.TabIndex = 1;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(21, 87);
            label4.Name = "label4";
            label4.Size = new Size(83, 15);
            label4.TabIndex = 7;
            label4.Text = "Camera Name";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(430, 87);
            label3.Name = "label3";
            label3.Size = new Size(63, 15);
            label3.TabIndex = 6;
            label3.Text = "Resolution";
            // 
            // cbxResolutions
            // 
            cbxResolutions.DropDownStyle = ComboBoxStyle.DropDownList;
            cbxResolutions.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            cbxResolutions.FormattingEnabled = true;
            cbxResolutions.Location = new Point(430, 105);
            cbxResolutions.Name = "cbxResolutions";
            cbxResolutions.Size = new Size(166, 29);
            cbxResolutions.TabIndex = 5;
            // 
            // btnSave
            // 
            btnSave.BackColor = SystemColors.HotTrack;
            btnSave.FlatAppearance.BorderColor = SystemColors.HotTrack;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnSave.ForeColor = Color.White;
            btnSave.Location = new Point(602, 105);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(110, 29);
            btnSave.TabIndex = 4;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = false;
            // 
            // picCameraPreview
            // 
            picCameraPreview.BackColor = Color.FromArgb(64, 64, 64);
            picCameraPreview.Location = new Point(21, 140);
            picCameraPreview.Name = "picCameraPreview";
            picCameraPreview.Size = new Size(691, 305);
            picCameraPreview.SizeMode = PictureBoxSizeMode.StretchImage;
            picCameraPreview.TabIndex = 3;
            picCameraPreview.TabStop = false;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.ForeColor = Color.Black;
            label2.Location = new Point(21, 16);
            label2.Name = "label2";
            label2.Size = new Size(230, 30);
            label2.TabIndex = 1;
            label2.Text = "Camera Configuration";
            // 
            // cbxCameras
            // 
            cbxCameras.DropDownStyle = ComboBoxStyle.DropDownList;
            cbxCameras.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            cbxCameras.FormattingEnabled = true;
            cbxCameras.Items.AddRange(new object[] { "Built-In Camera" });
            cbxCameras.Location = new Point(21, 105);
            cbxCameras.Name = "cbxCameras";
            cbxCameras.Size = new Size(403, 29);
            cbxCameras.TabIndex = 0;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(21, 46);
            label5.Name = "label5";
            label5.Size = new Size(424, 30);
            label5.TabIndex = 8;
            label5.Text = "Select your preferred camera from the list below, choose the desired resolution,\r\nand click 'Save' to store your configuration.";
            // 
            // WebCameraControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(pnlContent);
            Controls.Add(pnlHeader);
            Name = "WebCameraControl";
            Size = new Size(833, 541);
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
        private Label label2;
        private ComboBox cbxCameras;
        private Button btnSave;
        private ComboBox cbxResolutions;
        private Label label4;
        private Label label3;
        private Label label5;
    }
}
