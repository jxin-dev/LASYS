using LASYS.DesktopApp.Views.Interfaces;
using System.ComponentModel;

namespace LASYS.DesktopApp.Views.Forms
{
    public partial class ErrorForm : Form, IErrorView
    {
        private bool allowClose = false;

        public event EventHandler? RetryRequested;
        public event EventHandler? SkipRequested;
        public event EventHandler? StopBatchRequested;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string MessageText
        {
            get => lblMessage.Text;
            set => lblMessage.Text = value;
        }
        public ErrorForm()
        {
            InitializeComponent();
            btnRetry.Click += (s, e) => RetryRequested?.Invoke(this, EventArgs.Empty);
            btnSkip.Click += (s, e) => SkipRequested?.Invoke(this, EventArgs.Empty);
            btnStopBatch.Click += (s, e) => StopBatchRequested?.Invoke(this, EventArgs.Empty);
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
    }
}
