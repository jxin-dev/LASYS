namespace LASYS.DesktopApp.Views.Forms
{
    partial class ApprovalAuthenticationForm
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
            label1 = new Label();
            txtPassword = new TextBox();
            txtUsername = new TextBox();
            btnLogin = new Button();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            panel1 = new Panel();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.BackColor = Color.White;
            label1.Dock = DockStyle.Top;
            label1.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.FromArgb(0, 110, 100);
            label1.Image = Properties.Resources.verified_24;
            label1.ImageAlign = ContentAlignment.MiddleLeft;
            label1.Location = new Point(0, 0);
            label1.Name = "label1";
            label1.Size = new Size(373, 44);
            label1.TabIndex = 4;
            label1.Text = "     Approval";
            label1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtPassword
            // 
            txtPassword.Font = new Font("Segoe UI", 9.75F);
            txtPassword.Location = new Point(70, 169);
            txtPassword.Name = "txtPassword";
            txtPassword.PlaceholderText = "Enter password";
            txtPassword.Size = new Size(230, 25);
            txtPassword.TabIndex = 6;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // txtUsername
            // 
            txtUsername.Font = new Font("Segoe UI", 9.75F);
            txtUsername.Location = new Point(70, 121);
            txtUsername.Name = "txtUsername";
            txtUsername.PlaceholderText = "Enter username";
            txtUsername.Size = new Size(230, 25);
            txtUsername.TabIndex = 5;
            // 
            // btnLogin
            // 
            btnLogin.BackColor = Color.FromArgb(0, 140, 125);
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.Font = new Font("Segoe UI", 12F);
            btnLogin.ForeColor = Color.White;
            btnLogin.Location = new Point(112, 200);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(91, 37);
            btnLogin.TabIndex = 14;
            btnLogin.Text = "OK";
            btnLogin.UseVisualStyleBackColor = false;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold);
            label2.ForeColor = Color.FromArgb(0, 110, 100);
            label2.Location = new Point(70, 101);
            label2.Name = "label2";
            label2.Size = new Size(69, 17);
            label2.TabIndex = 15;
            label2.Text = "Username";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold);
            label3.ForeColor = Color.FromArgb(0, 110, 100);
            label3.Location = new Point(70, 149);
            label3.Name = "label3";
            label3.Size = new Size(66, 17);
            label3.TabIndex = 16;
            label3.Text = "Password";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label4.ForeColor = Color.FromArgb(0, 110, 100);
            label4.Location = new Point(70, 60);
            label4.Name = "label4";
            label4.Size = new Size(228, 34);
            label4.TabIndex = 17;
            label4.Text = "Authorization is required to continue. \r\nEnter your credentials below.  ";
            // 
            // panel1
            // 
            panel1.BackColor = Color.FromArgb(0, 110, 100);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 275);
            panel1.Name = "panel1";
            panel1.Size = new Size(373, 5);
            panel1.TabIndex = 8;
            // 
            // btnCancel
            // 
            btnCancel.BackColor = Color.FromArgb(220, 230, 228);
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 12F);
            btnCancel.ForeColor = Color.FromArgb(60, 60, 60);
            btnCancel.Location = new Point(209, 200);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(91, 37);
            btnCancel.TabIndex = 18;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = false;
            // 
            // ApprovalAuthenticationForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(373, 280);
            Controls.Add(btnCancel);
            Controls.Add(panel1);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(btnLogin);
            Controls.Add(txtPassword);
            Controls.Add(txtUsername);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.None;
            Name = "ApprovalAuthenticationForm";
            Text = "Approval";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox txtPassword;
        private TextBox txtUsername;
        private Button btnLogin;
        private Label label2;
        private Label label3;
        private Label label4;
        private Panel panel1;
        private Button btnCancel;
    }
}