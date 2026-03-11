using LASYS.Application.Common.Enums;
using LASYS.DesktopApp.Views.Interfaces;

namespace LASYS.DesktopApp.Views.Forms
{
    public partial class ErrorForm : Form, IErrorView
    {
        private bool allowClose = false;

        public event EventHandler<OperatorDecision>? DecisionRequested;

        public string MessageText
        {
            get => lblMessage.Text;
            set => lblMessage.Text = value;
        }
        public ErrorForm()
        {
            InitializeComponent();

            btnRetry.Click += (s, e) => DecisionRequested?.Invoke(this, OperatorDecision.Retry);

            btnSkip.Click += (s, e) => DecisionRequested?.Invoke(this, OperatorDecision.Skip);

            btnStopBatch.Click += (s, e) => DecisionRequested?.Invoke(this, OperatorDecision.Stop);
        }

        public void CloseError()
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
    }
}
