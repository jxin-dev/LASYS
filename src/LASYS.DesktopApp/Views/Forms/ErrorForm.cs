using LASYS.Application.Features.BatchPrinting.Enums;
using LASYS.DesktopApp.Views.Interfaces;

namespace LASYS.DesktopApp.Views.Forms
{
    public partial class ErrorForm : Form, IErrorView
    {
        private bool allowClose = false;

        public event EventHandler<StepResult>? DecisionRequested;

        public string MessageText
        {
            get => lblMessage.Text;
            set => lblMessage.Text = value;
        }
        public ErrorForm()
        {
            InitializeComponent();

            btnRetry.Click += (s, e) => DecisionRequested?.Invoke(this, StepResult.Retry);

            btnSkip.Click += (s, e) => DecisionRequested?.Invoke(this, StepResult.Skip);

            btnStopBatch.Click += (s, e) => DecisionRequested?.Invoke(this, StepResult.Stop);
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
