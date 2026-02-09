using LASYS.DesktopApp.Presenters;
using LASYS.DesktopApp.Views.Interfaces;
using LASYS.UIControls.Controls;

namespace LASYS.DesktopApp.Views.Forms
{
    public partial class SplashForm : Form, ISplashView
    {
        private readonly CustomProgressBar _progressBar;

        public event EventHandler? ViewShown;

        public SplashForm()
        {
            InitializeComponent();
            //_presenter = presenter;

            _progressBar = new CustomProgressBar
            {
                Dock = DockStyle.Bottom,
                ProgressColor = Color.FromArgb(255, 0, 255, 255), //Color.FromArgb(0, 122, 204),
                ProgressBackgroundColor = Color.LightGray,
                ShowPercentage = false
            };

            pnlLoadingContainer.Controls.Add(_progressBar);

            UpdateCopyrightText();
        }

        private void UpdateCopyrightText()
        {
            int year = DateTime.Now.Year;
            lblCopyright.Text = $"Copyright © {year} Innovathink Corporation.\nAll rights reserved.";
        }

        public void CloseView()
        {
            if (InvokeRequired)
            {
                Invoke(Close);
                return;
            }
            Close();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            //_presenter.AttachView(this);
            ViewShown?.Invoke(this, EventArgs.Empty);
            //await _presenter.InitializeAsync();
        }


        public void UpdateProgress(int percent, string message)
        {
            _progressBar.Value = percent;
            _progressBar.UpdateStatus(message);
        }

        public void ShowView() => Show();// System.Windows.Forms.Application.Run(this);
        public void HideView() => Hide();
    }
}
