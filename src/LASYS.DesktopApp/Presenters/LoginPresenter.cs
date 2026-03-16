using LASYS.DesktopApp.Views.Forms;
using LASYS.DesktopApp.Views.Interfaces;
using LASYS.Infrastructure.Persistence.Connection;
using MediatR;
using Velopack;
using Velopack.Sources;

namespace LASYS.DesktopApp.Presenters
{
    public class LoginPresenter
    {
        private ILoginView _view;
        private readonly DatabaseSettings _databaseSettings;
        private readonly IMediator _mediator;
        public LoginForm View { get; }
        public LoginPresenter(ILoginView view, IMediator mediator, DatabaseSettings databaseSettings)
        {
            _view = view;
            View = (LoginForm)view;
            _mediator = mediator;
            _databaseSettings = databaseSettings;

            _view.LoginClicked += OnLoginClicked;
            _view.CheckForUpdatesRequested += OnCheckForUpdatesRequested;
        }
        private async Task OnCheckForUpdatesRequested(object? sender, EventArgs e)
        {
            await CheckForUpdatesAsync();
        }

        private async Task OnLoginClicked(object? sender, EventArgs e)
        {
            _databaseSettings.Environment = _view.SelectedEnvironment;

            if (string.IsNullOrWhiteSpace(_view.Username) || string.IsNullOrWhiteSpace(_view.Password))
            {
                _view.ShowMessage("Please enter both username and password.");
                return;
            }

#if (DEBUG)
            _view.SetDialogResult(DialogResult.OK);
#else

            var result = await _mediator.Send(new LoginQuery(_view.Username, _view.Password));
            if (result.IsSuccess)
            {
                _view.SetDialogResult(DialogResult.OK);
            }
            else
            {
                _view.ShowMessage(result.ErrorOrDefault);
            }

#endif

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
