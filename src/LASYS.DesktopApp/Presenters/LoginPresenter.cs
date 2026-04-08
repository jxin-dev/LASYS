using LASYS.Application.Common.Messaging;
using LASYS.Application.Features.Authentication.Login;
using LASYS.Application.Interfaces.Context;
using LASYS.Application.Interfaces.Services;
using LASYS.DesktopApp.Views.Forms;
using LASYS.DesktopApp.Views.Interfaces;
using LASYS.Infrastructure.Persistence.Connection;
using LASYS.Infrastructure.Services.Session;
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
        private readonly ICurrentUser _currentUser;
        private readonly ILogService _logService;
        private readonly ISessionTracker _sessionTracker;
        private bool _isLoggingIn = false;
        public LoginForm View { get; }
        public LoginPresenter(ILoginView view, IMediator mediator, DatabaseSettings databaseSettings, ICurrentUser currentUser, ILogService logService, ISessionTracker sessionTracker)
        {
            _view = view;
            View = (LoginForm)view;

            _mediator = mediator;
            _databaseSettings = databaseSettings;
            _currentUser = currentUser;
            _logService = logService;

            _view.LoginClicked += OnLoginClicked;
            _view.CheckForUpdatesRequested += OnCheckForUpdatesRequested;
            _sessionTracker = sessionTracker;
        }


        private async Task OnCheckForUpdatesRequested(object? sender, EventArgs e)
        {
            await CheckForUpdatesAsync();
        }

        private async Task OnLoginClicked(object? sender, EventArgs e)
        {
            if (_isLoggingIn)
                return;

            var environment = _view.SelectedEnvironment;
            var username = _view.Username?.Trim();
            var password = _view.Password;

            if (string.IsNullOrWhiteSpace(environment))
            {
                _view.ShowMessage("Please select an environment.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_view.Username) || string.IsNullOrWhiteSpace(_view.Password))
            {
                _view.ShowMessage("Please enter both username and password.");
                return;
            }

            _databaseSettings.Environment = environment;
            _isLoggingIn = true;

            try
            {
                _view.SetLoginEnabled(false);
                _view.SetStatus("Logging in...");
                var result = await _mediator.Send(new LoginQuery(_view.Username, _view.Password));
                if (result.IsSuccess)
                {
                    var user = result.Value!;
                    
                    _currentUser.SetUser(
                        user.USER_CODE,
                        user.USER_NAME,
                        user.SECTION_ID,
                        user.ROLE_CODE,
                        user.PLANT_CODE,
                        user.FIRST_NAME,
                        user.LAST_NAME,
                        user.MIDDLE_NAME,
                        user.IMAGE_PATH);

                    _sessionTracker.StartSession();

                    _view.SetDialogResult(DialogResult.OK);
                }
                else
                {
                    _view.ShowMessage(result.ErrorOrDefault);
                }
            }
            finally
            {
                _view.SetLoginEnabled(true);
                _view.SetStatus("Login");
                _isLoggingIn = false;
            }
           
            //_view.SetDialogResult(DialogResult.OK);

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
