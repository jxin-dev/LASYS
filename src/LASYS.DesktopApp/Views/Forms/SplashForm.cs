using LASYS.DesktopApp.Presenters;
using LASYS.DesktopApp.Views.Interfaces;

namespace LASYS.DesktopApp.Views.Forms
{
    public partial class SplashForm : Form, ISplashView
    {
        private readonly SplashPresenter _presenter;
        public SplashForm(SplashPresenter presenter)
        {
            InitializeComponent();
            _presenter = presenter;
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

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _presenter.AttachView(this);
            await _presenter.InitializeAsync();
        }


        public void UpdateProgress(int percent, string message)
        {
            progressBar.Value = percent;
            lblStatus.Text = message;
            lblStatus.Refresh();
        }

        public void ShowView() => Application.Run(this);
        public void HideView() => Hide();
    }
}
