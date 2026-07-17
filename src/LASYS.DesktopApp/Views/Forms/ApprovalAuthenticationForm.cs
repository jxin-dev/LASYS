using LASYS.Application.Features.BatchPrinting.Events;
using LASYS.DesktopApp.Views.Interfaces;

namespace LASYS.DesktopApp.Views.Forms
{
    public partial class ApprovalAuthenticationForm : Form, IApprovalAuthenticationView
    {
        private bool allowClose = false;
        public event EventHandler<ApprovalCredentialsEventArgs>? ApprovalRequested;
        public event EventHandler? ApprovalCancelled;

        public ApprovalAuthenticationForm()
        {
            InitializeComponent();
            btnLogin.Click += (sender, e) =>
            {
                var username = txtUsername.Text;
                var password = txtPassword.Text;
                ApprovalRequested?.Invoke(this, new ApprovalCredentialsEventArgs(username, password));
            };

            Shown += (s, e) => btnCancel.Focus();

            btnCancel.Click += (sender, e) =>
            {
                ApprovalCancelled?.Invoke(this, EventArgs.Empty);
            }; 
        }


        public void ApprovalFailed(string errorMessage)
        {
            MessageBox.Show(errorMessage, "Approval Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ApprovalSucceeded()
        {
            allowClose = true;
            Close();
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!allowClose && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true; // Block Alt+F4, X button, etc.
            }
        }
        public void InvokeOnUI(Action action)
        {
            if (this.InvokeRequired)
                this.Invoke(action);
            else
                action();
        }

        public void CloseApproval()
        {
            allowClose = true;
            Close();
        }
    }
}
