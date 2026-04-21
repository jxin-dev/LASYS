using LASYS.DesktopApp.Helpers;
using LASYS.DesktopApp.Views.Interfaces;

namespace LASYS.DesktopApp.Views.Forms
{
    public partial class LoginForm : Form, ILoginView
    {
        public event Func<object?, EventArgs, Task>? LoginClicked;
        public event Func<object?, EventArgs, Task>? CheckForUpdatesRequested;
        public string Username => txtUsername.Text;
        public string Password => txtPassword.Text;

        public string? SelectedEnvironment => cbxEnvironment?.SelectedItem?.ToString();

        public LoginForm()
        {
            InitializeComponent();
            btnLogin.MouseEnter += (_, _) => btnLogin.BackColor = Color.FromArgb(0, 166, 147);
            btnLogin.MouseLeave += (_, _) => btnLogin.BackColor = Color.FromArgb(0, 140, 125);

            btnLogin.Click += (s, e) => LoginClicked?.Invoke(this, EventArgs.Empty);
            btnCancel.Click += (s, e) => Close();

            chkShowPassword.CheckedChanged += (s, e) =>
            {
                txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
            };

            lnklblCheckUpdates.Text = AppVersionHelper.GetVersion();
            lnklblCheckUpdates.Click += (s, e) => CheckForUpdatesRequested?.Invoke(this, EventArgs.Empty);


            cbxEnvironment.SelectedItem = "Production";
            Shown += (s, e) => cbxEnvironment.Focus();
        }

        public void ShowMessage(string errorMessage)
        {
            MessageBox.Show(errorMessage, "Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ShowStatusMessage(string message, MessageBoxIcon icon)
        {
            MessageBox.Show(message, "Check for updates", MessageBoxButtons.OK, icon);

        }

        public void SetDialogResult(DialogResult result)
        {
            this.DialogResult = result;
        }

        public void SetLoginEnabled(bool enabled)
        {
            btnLogin.Enabled = enabled;
        }

        public void SetStatus(string status)
        {
            this.Text = status;
        }
    }
}
