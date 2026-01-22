using System.Threading.Tasks;
using LASYS.DesktopApp.Core.Interfaces;
using LASYS.DesktopApp.Presenters.Interfaces;
using LASYS.DesktopApp.Views.Interfaces;
using Velopack;
using Velopack.Sources;

namespace LASYS.DesktopApp.Presenters
{
    public class LoginPresenter : ILoginPresenter
    {
        private ILoginView? _view;
        private readonly IViewFactory _factory;
        public LoginPresenter(IViewFactory factory)
        {
            _factory = factory;
        }

        public void AttachView(ILoginView view)
        {
            _view = view;
            _view.LoginClicked += OnLoginClicked;
            _view.CheckForUpdatesRequested +=  OnCheckForUpdatesRequested;
        }

        private async Task OnCheckForUpdatesRequested(object? sender, EventArgs e)
        {
            await CheckForUpdatesAsync();
        }

        private void OnLoginClicked(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_view?.Username) || string.IsNullOrWhiteSpace(_view?.Password))
            {
                _view?.ShowMessage("Please enter both username and password.");
                return;
            }

            if (_view!.Username == "admin" && _view.Password == "1234")
            {
                _view.HideView();
                var main = _factory.Create<IMainView, MainPresenter>();
                main.ShowView();
            }
            else
            {
                _view.ShowMessage("Invalid username or password.");
            }
        }

        public async Task CheckForUpdatesAsync()
        {
            try
            {
                // Point to your GitHub Repository 
                var source = new GithubSource("https://github.com/jxin-dev/LASYS", null, false);
                var mgr = new UpdateManager(source);

                // 1. Check for new version 
                var newVersion = await mgr.CheckForUpdatesAsync();
                if (newVersion == null)
                {
                    _view?.ShowStatusMessage("Application is up-to-date.", MessageBoxIcon.Information);
                    return;
                }

                // 2. Download the update 
                await mgr.DownloadUpdatesAsync(newVersion);

                // 3. Install and restart 
                mgr.ApplyUpdatesAndRestart(newVersion);
            }
            catch (Velopack.Exceptions.NotInstalledException)
            {
                _view?.ShowStatusMessage("Debug mode – updates disabled.", MessageBoxIcon.Information);
            }
            catch (Exception)
            {
                _view?.ShowStatusMessage("Unable to access the update server.\nPlease check your internet connection or try again later.", MessageBoxIcon.Exclamation);
            }
        }
    }
}
