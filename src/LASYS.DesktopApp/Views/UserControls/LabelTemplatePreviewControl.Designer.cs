namespace LASYS.DesktopApp.Views.UserControls
{
    partial class LabelTemplatePreviewControl
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
            pnlPreview = new Panel();
            ((System.ComponentModel.ISupportInitialize)picPreview).BeginInit();
            pnlPreview.SuspendLayout();
            SuspendLayout();
            // 
            // picPreview
            // 
            picPreview.BackColor = SystemColors.ControlDarkDark;
            picPreview.Location = new Point(3, 3);
            picPreview.Name = "picPreview";
            picPreview.Size = new Size(415, 266);
            picPreview.TabIndex = 1;
            picPreview.TabStop = false;
            // 
            // pnlPreview
            // 
            pnlPreview.Controls.Add(picPreview);
            pnlPreview.Dock = DockStyle.Fill;
            pnlPreview.Location = new Point(0, 0);
            pnlPreview.Name = "pnlPreview";
            pnlPreview.Size = new Size(421, 272);
            pnlPreview.TabIndex = 2;
            // 
            // LabelTemplatePreviewControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(pnlPreview);
            Name = "LabelTemplatePreviewControl";
            Size = new Size(421, 272);
            ((System.ComponentModel.ISupportInitialize)picPreview).EndInit();
            pnlPreview.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private PictureBox picPreview;
        private Panel pnlPreview;
    }
}
