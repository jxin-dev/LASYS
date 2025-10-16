using LASYS.DesktopApp.Presenters.Interfaces;
using LASYS.DesktopApp.Views.Interfaces;

namespace LASYS.DesktopApp.Views.Forms
{
    public partial class SplashForm : Form, ISplashView
    {
        private ISplashPresenter? _presenter;
        public SplashForm()
        {
            InitializeComponent();
        }

        public void CloseView()
        {
            Invoke(() => Close());
        }

        public void SetPresenter(ISplashPresenter presenter)
        {
            _presenter = presenter;
        }
        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (_presenter != null)
                await _presenter.StartLoadingAsync();   
        }

        public void UpdateProgress(int percent, string message)
        {
            progressBar.Value = percent;
            lblStatus.Text = message;
        }

    }
}
