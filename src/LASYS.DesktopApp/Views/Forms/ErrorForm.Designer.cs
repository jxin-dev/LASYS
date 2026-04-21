namespace LASYS.DesktopApp.Views.Forms
{
    partial class ErrorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ErrorForm));
            btnRetry = new Button();
            btnSkip = new Button();
            btnStopBatch = new Button();
            label1 = new Label();
            lblMessage = new Label();
            SuspendLayout();
            // 
            // btnRetry
            // 
            btnRetry.BackColor = Color.DodgerBlue;
            btnRetry.FlatAppearance.BorderSize = 0;
            btnRetry.FlatStyle = FlatStyle.Flat;
            btnRetry.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold);
            btnRetry.ForeColor = Color.White;
            btnRetry.Image = Properties.Resources.retry24;
            btnRetry.Location = new Point(39, 362);
            btnRetry.Margin = new Padding(4, 5, 4, 5);
            btnRetry.Name = "btnRetry";
            btnRetry.Size = new Size(133, 73);
            btnRetry.TabIndex = 0;
            btnRetry.Text = "Retry";
            btnRetry.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnRetry.UseVisualStyleBackColor = false;
            // 
            // btnSkip
            // 
            btnSkip.BackColor = Color.DarkGray;
            btnSkip.FlatAppearance.BorderSize = 0;
            btnSkip.FlatStyle = FlatStyle.Flat;
            btnSkip.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold);
            btnSkip.Image = Properties.Resources.skip24;
            btnSkip.Location = new Point(180, 362);
            btnSkip.Margin = new Padding(4, 5, 4, 5);
            btnSkip.Name = "btnSkip";
            btnSkip.Size = new Size(139, 73);
            btnSkip.TabIndex = 1;
            btnSkip.Text = "Skip";
            btnSkip.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnSkip.UseVisualStyleBackColor = false;
            // 
            // btnStopBatch
            // 
            btnStopBatch.BackColor = Color.Crimson;
            btnStopBatch.FlatAppearance.BorderSize = 0;
            btnStopBatch.FlatStyle = FlatStyle.Flat;
            btnStopBatch.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold);
            btnStopBatch.ForeColor = Color.White;
            btnStopBatch.Image = Properties.Resources.stopbatch24;
            btnStopBatch.Location = new Point(371, 362);
            btnStopBatch.Margin = new Padding(4, 5, 4, 5);
            btnStopBatch.Name = "btnStopBatch";
            btnStopBatch.Size = new Size(259, 73);
            btnStopBatch.TabIndex = 2;
            btnStopBatch.Text = "Stop Batch Priniting";
            btnStopBatch.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnStopBatch.UseVisualStyleBackColor = false;
            // 
            // label1
            // 
            label1.BackColor = Color.Firebrick;
            label1.Dock = DockStyle.Top;
            label1.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.White;
            label1.Image = Properties.Resources.error24;
            label1.ImageAlign = ContentAlignment.MiddleLeft;
            label1.Location = new Point(0, 0);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(669, 73);
            label1.TabIndex = 3;
            label1.Text = "     Operation Failed";
            label1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblMessage
            // 
            lblMessage.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblMessage.Location = new Point(39, 105);
            lblMessage.Margin = new Padding(4, 0, 4, 0);
            lblMessage.Name = "lblMessage";
            lblMessage.Size = new Size(591, 212);
            lblMessage.TabIndex = 4;
            lblMessage.Text = "Failed to print the label. Please check the printer connection and try again";
            lblMessage.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // ErrorForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(669, 462);
            Controls.Add(lblMessage);
            Controls.Add(label1);
            Controls.Add(btnStopBatch);
            Controls.Add(btnSkip);
            Controls.Add(btnRetry);
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 5, 4, 5);
            Name = "ErrorForm";
            Text = "ErrorForm";
            ResumeLayout(false);
        }

        #endregion

        private Button btnRetry;
        private Button btnSkip;
        private Button btnStopBatch;
        private Label label1;
        private Label lblMessage;
    }
}