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
            txtUsername = new TextBox();
            txtPassword = new TextBox();
            btnLogin = new Button();
            chkShowPassword = new CheckBox();
            comboBox1 = new ComboBox();
            label1 = new Label();
            btnCancel = new Button();
            pictureBox1 = new PictureBox();
            label2 = new Label();
            panel1 = new Panel();
            panel2 = new Panel();
            lnklblCheckUpdates = new LinkLabel();
            panel3 = new Panel();
            label3 = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(272, 108);
            txtUsername.Name = "txtUsername";
            txtUsername.PlaceholderText = "Username";
            txtUsername.Size = new Size(223, 23);
            txtUsername.TabIndex = 0;
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(272, 137);
            txtPassword.Name = "txtPassword";
            txtPassword.PlaceholderText = "Password";
            txtPassword.Size = new Size(223, 23);
            txtPassword.TabIndex = 1;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // btnLogin
            // 
            btnLogin.Location = new Point(339, 196);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(75, 23);
            btnLogin.TabIndex = 2;
            btnLogin.Text = "OK";
            btnLogin.UseVisualStyleBackColor = true;
            // 
            // chkShowPassword
            // 
            chkShowPassword.AutoSize = true;
            chkShowPassword.Location = new Point(272, 166);
            chkShowPassword.Name = "chkShowPassword";
            chkShowPassword.Size = new Size(108, 19);
            chkShowPassword.TabIndex = 3;
            chkShowPassword.Text = "Show Password";
            chkShowPassword.UseVisualStyleBackColor = true;
            // 
            // comboBox1
            // 
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "Production Database", "Test Database" });
            comboBox1.Location = new Point(339, 79);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(156, 23);
            comboBox1.TabIndex = 4;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(272, 82);
            label1.Name = "label1";
            label1.Size = new Size(61, 15);
            label1.TabIndex = 5;
            label1.Text = "Log on to:";
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(420, 196);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 6;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
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
            // panel2
            // 
            panel2.BackColor = Color.SeaGreen;
            panel2.Controls.Add(lnklblCheckUpdates);
            panel2.Dock = DockStyle.Bottom;
            panel2.Location = new Point(0, 254);
            panel2.Name = "panel2";
            panel2.Size = new Size(570, 38);
            panel2.TabIndex = 10;
            // 
            // lnklblCheckUpdates
            // 
            lnklblCheckUpdates.ActiveLinkColor = Color.DarkRed;
            lnklblCheckUpdates.AutoSize = true;
            lnklblCheckUpdates.LinkBehavior = LinkBehavior.HoverUnderline;
            lnklblCheckUpdates.LinkColor = Color.White;
            lnklblCheckUpdates.Location = new Point(12, 14);
            lnklblCheckUpdates.Name = "lnklblCheckUpdates";
            lnklblCheckUpdates.Size = new Size(72, 15);
            lnklblCheckUpdates.TabIndex = 0;
            lnklblCheckUpdates.TabStop = true;
            lnklblCheckUpdates.Text = "Version 1.0.0";
            lnklblCheckUpdates.VisitedLinkColor = Color.White;
            // 
            // panel3
            // 
            panel3.BackColor = Color.SeaGreen;
            panel3.Dock = DockStyle.Top;
            panel3.Location = new Point(0, 0);
            panel3.Name = "panel3";
            panel3.Size = new Size(570, 12);
            panel3.TabIndex = 11;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.ForeColor = Color.SeaGreen;
            label3.Location = new Point(82, 205);
            label3.Name = "label3";
            label3.Size = new Size(129, 17);
            label3.TabIndex = 12;
            label3.Text = "(New Label Design)";
            label3.TextAlign = ContentAlignment.TopCenter;
            // 
            // LoginForm
            // 
            AcceptButton = btnLogin;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.GhostWhite;
            CancelButton = btnCancel;
            ClientSize = new Size(570, 292);
            Controls.Add(label3);
            Controls.Add(panel3);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Controls.Add(label2);
            Controls.Add(btnCancel);
            Controls.Add(label1);
            Controls.Add(comboBox1);
            Controls.Add(chkShowPassword);
            Controls.Add(btnLogin);
            Controls.Add(txtPassword);
            Controls.Add(txtUsername);
            Controls.Add(pictureBox1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "LoginForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Login";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private CheckBox chkShowPassword;
        private ComboBox comboBox1;
        private Label label1;
        private Button btnCancel;
        private PictureBox pictureBox1;
        private Label label2;
        private Panel panel1;
        private Panel panel2;
        private Panel panel3;
        private Label label3;
        private LinkLabel lnklblCheckUpdates;
    }
}