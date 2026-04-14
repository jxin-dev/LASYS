namespace LASYS.DesktopApp.Views.Forms
{
    partial class LoginForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginForm));
            txtUsername = new TextBox();
            txtPassword = new TextBox();
            btnLogin = new Button();
            chkShowPassword = new CheckBox();
            cbxEnvironment = new ComboBox();
            label1 = new Label();
            pictureBox1 = new PictureBox();
            label2 = new Label();
            panel1 = new Panel();
            panelBottom = new Panel();
            lnklblCheckUpdates = new LinkLabel();
            panelTop = new Panel();
            btnCancel = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            panelBottom.SuspendLayout();
            SuspendLayout();
            // 
            // txtUsername
            // 
            txtUsername.Font = new Font("Segoe UI", 9.75F);
            txtUsername.Location = new Point(272, 97);
            txtUsername.Name = "txtUsername";
            txtUsername.PlaceholderText = "Username";
            txtUsername.Size = new Size(230, 25);
            txtUsername.TabIndex = 0;
            // 
            // txtPassword
            // 
            txtPassword.Font = new Font("Segoe UI", 9.75F);
            txtPassword.Location = new Point(272, 126);
            txtPassword.Name = "txtPassword";
            txtPassword.PlaceholderText = "Password";
            txtPassword.Size = new Size(230, 25);
            txtPassword.TabIndex = 1;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // btnLogin
            // 
            btnLogin.BackColor = Color.FromArgb(0, 140, 125);
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.Font = new Font("Segoe UI", 12F);
            btnLogin.ForeColor = Color.White;
            btnLogin.Location = new Point(314, 194);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(91, 37);
            btnLogin.TabIndex = 2;
            btnLogin.Text = "OK";
            btnLogin.UseVisualStyleBackColor = false;
            // 
            // chkShowPassword
            // 
            chkShowPassword.AutoSize = true;
            chkShowPassword.Font = new Font("Segoe UI", 9.75F);
            chkShowPassword.ForeColor = Color.FromArgb(60, 60, 60);
            chkShowPassword.Location = new Point(272, 155);
            chkShowPassword.Name = "chkShowPassword";
            chkShowPassword.Size = new Size(118, 21);
            chkShowPassword.TabIndex = 3;
            chkShowPassword.Text = "Show Password";
            chkShowPassword.UseVisualStyleBackColor = true;
            // 
            // cbxEnvironment
            // 
            cbxEnvironment.DropDownStyle = ComboBoxStyle.DropDownList;
            cbxEnvironment.Font = new Font("Segoe UI", 9.75F);
            cbxEnvironment.FormattingEnabled = true;
            cbxEnvironment.Items.AddRange(new object[] { "Production", "Staging" });
            cbxEnvironment.Location = new Point(272, 68);
            cbxEnvironment.Name = "cbxEnvironment";
            cbxEnvironment.Size = new Size(230, 25);
            cbxEnvironment.TabIndex = 4;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9.75F);
            label1.ForeColor = Color.FromArgb(60, 60, 60);
            label1.Location = new Point(272, 47);
            label1.Name = "label1";
            label1.Size = new Size(68, 17);
            label1.TabIndex = 5;
            label1.Text = "Log on to:";
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.terumo;
            pictureBox1.Location = new Point(50, 47);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(185, 170);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 7;
            pictureBox1.TabStop = false;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.ForeColor = Color.SeaGreen;
            label2.Location = new Point(59, 182);
            label2.Name = "label2";
            label2.Size = new Size(176, 25);
            label2.TabIndex = 8;
            label2.Text = "LASYS Application";
            label2.TextAlign = ContentAlignment.TopCenter;
            // 
            // panel1
            // 
            panel1.BackColor = Color.DarkRed;
            panel1.Location = new Point(252, 51);
            panel1.Name = "panel1";
            panel1.Size = new Size(1, 170);
            panel1.TabIndex = 9;
            // 
            // panelBottom
            // 
            panelBottom.BackColor = Color.FromArgb(0, 140, 125);
            panelBottom.Controls.Add(lnklblCheckUpdates);
            panelBottom.Dock = DockStyle.Bottom;
            panelBottom.Location = new Point(0, 254);
            panelBottom.Name = "panelBottom";
            panelBottom.Size = new Size(570, 38);
            panelBottom.TabIndex = 10;
            // 
            // lnklblCheckUpdates
            // 
            lnklblCheckUpdates.ActiveLinkColor = Color.DarkRed;
            lnklblCheckUpdates.AutoSize = true;
            lnklblCheckUpdates.ForeColor = Color.FromArgb(120, 120, 120);
            lnklblCheckUpdates.LinkBehavior = LinkBehavior.HoverUnderline;
            lnklblCheckUpdates.LinkColor = Color.White;
            lnklblCheckUpdates.Location = new Point(12, 14);
            lnklblCheckUpdates.Name = "lnklblCheckUpdates";
            lnklblCheckUpdates.Size = new Size(37, 15);
            lnklblCheckUpdates.TabIndex = 0;
            lnklblCheckUpdates.TabStop = true;
            lnklblCheckUpdates.Text = "v1.0.0";
            lnklblCheckUpdates.VisitedLinkColor = Color.White;
            // 
            // panelTop
            // 
            panelTop.BackColor = Color.FromArgb(0, 140, 125);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(0, 0);
            panelTop.Name = "panelTop";
            panelTop.Size = new Size(570, 12);
            panelTop.TabIndex = 11;
            // 
            // btnCancel
            // 
            btnCancel.BackColor = Color.FromArgb(220, 230, 228);
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 12F);
            btnCancel.ForeColor = Color.FromArgb(60, 60, 60);
            btnCancel.Location = new Point(411, 194);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(91, 37);
            btnCancel.TabIndex = 13;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = false;
            // 
            // LoginForm
            // 
            AcceptButton = btnLogin;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 248, 247);
            ClientSize = new Size(570, 292);
            Controls.Add(btnCancel);
            Controls.Add(panelTop);
            Controls.Add(panelBottom);
            Controls.Add(panel1);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(cbxEnvironment);
            Controls.Add(chkShowPassword);
            Controls.Add(btnLogin);
            Controls.Add(txtPassword);
            Controls.Add(txtUsername);
            Controls.Add(pictureBox1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "LoginForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Login";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            panelBottom.ResumeLayout(false);
            panelBottom.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private CheckBox chkShowPassword;
        private ComboBox cbxEnvironment;
        private Label label1;
        private PictureBox pictureBox1;
        private Label label2;
        private Panel panel1;
        private Panel panelBottom;
        private Panel panelTop;
        private LinkLabel lnklblCheckUpdates;
        private Button btnCancel;
    }
}