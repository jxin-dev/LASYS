namespace LASYS.DesktopApp.Views.Forms
{
    partial class LabelBoxTypeForm
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
            flowButtons = new FlowLayoutPanel();
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
            lblHeader.Size = new Size(250, 44);
            lblHeader.TabIndex = 4;
            lblHeader.Text = "Label Box Type";
            lblHeader.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // flowButtons
            // 
            flowButtons.Dock = DockStyle.Top;
            flowButtons.Location = new Point(0, 44);
            flowButtons.Name = "flowButtons";
            flowButtons.Size = new Size(250, 70);
            flowButtons.TabIndex = 7;
            // 
            // LabelBoxTypeForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(250, 125);
            Controls.Add(flowButtons);
            Controls.Add(lblHeader);
            FormBorderStyle = FormBorderStyle.None;
            Name = "LabelBoxTypeForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "LabelBoxTypeForm";
            ResumeLayout(false);
        }

        #endregion

        private Label lblHeader;
        private FlowLayoutPanel flowButtons;
    }
}