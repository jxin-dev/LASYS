using LASYS.DesktopApp.Presenters.Interfaces;
using LASYS.DesktopApp.Views.Interfaces;

namespace LASYS.DesktopApp.Views.Forms
{
    public partial class LoginForm : Form, ILoginView
    {
        public event EventHandler? LoginClicked;
        public event Func<object?, EventArgs, Task>? CheckForUpdatesRequested;

        public string Username => txtUsername.Text;
        public string Password => txtPassword.Text;

        public LoginForm()
        {
            InitializeComponent();
            btnLogin.Click += (s, e) => LoginClicked?.Invoke(this, EventArgs.Empty);
            chkShowPassword.CheckedChanged += (s, e) =>
            {
                txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
            };

            lnklblCheckUpdates.Click += (s, e) => CheckForUpdatesRequested?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            CloseView();
        }
        public void CloseView() => System.Windows.Forms.Application.Exit();
        public void ShowView() => Show();
        public void HideView() => Hide();

        public void ShowMessage(string errorMessage)
        {
            MessageBox.Show(errorMessage, "Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ShowStatusMessage(string message, MessageBoxIcon icon)
        {
            MessageBox.Show(message, "Check for updates", MessageBoxButtons.OK, icon);

        }
    }
}
