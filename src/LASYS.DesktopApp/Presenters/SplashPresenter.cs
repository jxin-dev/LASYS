using LASYS.Camera.Events;
using LASYS.Camera.Interfaces;
using LASYS.DesktopApp.Core.Interfaces;
using LASYS.DesktopApp.Presenters.Interfaces;
using LASYS.DesktopApp.Views.Interfaces;

namespace LASYS.DesktopApp.Presenters
{
    public class SplashPresenter //: ISplashPresenter
    {
        private ISplashView _view;
        private readonly ICameraConfig _cameraConfig;
        private readonly ICameraService _cameraService;

        public SplashPresenter(ISplashView view, ICameraConfig cameraConfig, ICameraService cameraService)
        {
            _view = view;
            _view.ViewShown += OnViewShown;

            _cameraConfig = cameraConfig;
            _cameraService = cameraService;

            _cameraConfig.CameraConfigIssue += OnCameraConfigIssue;
        }

        private async void OnViewShown(object? sender, EventArgs e)
        {
            await InitializeAsync();

        }

        //public SplashPresenter(IViewFactory factory, ICameraConfig cameraConfig, ICameraService cameraService)
        //{
        //    _factory = factory;
        //    _cameraConfig = cameraConfig;
        //    _cameraService = cameraService;

        //    _cameraConfig.CameraConfigIssue += OnCameraConfigIssue;
        //}
        private void OnCameraConfigIssue(object? sender, CameraConfigEventArgs e)
        {
            if (_view == null) return;
            // Ensure UI thread update if needed
            _view.UpdateProgress(15, e.Message);
        }

        //public void AttachView(ISplashView view)
        //{
        //    _view = view;
        //}
        public async Task InitializeAsync()
        {
            _view?.UpdateProgress(0, "Loading, Please wait...");
            await Task.Delay(500);

            _view?.UpdateProgress(10, "Checking for updates...");
            await Task.Delay(500);
            // Load camera configuration
            try
            {
                var config = await _cameraConfig.LoadAsync();
                var camera = _cameraService.ResolveCamera(config);

                await _cameraService.InitializeAsync();
                if (camera != null)
                    _view?.UpdateProgress(15, $"Camera \"{camera.Name}\" connected successfully.");
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                _view?.UpdateProgress(15, $"Failed to load the saved camera configuration: {ex.Message}");
            }
            finally
            {
                // Always unsubscribe
                _cameraConfig.CameraConfigIssue -= OnCameraConfigIssue;
            }


            _view?.UpdateProgress(80, "Finalizing setup...");
            await Task.Delay(2000);
            _view?.UpdateProgress(100, "Launching application...");
            await Task.Delay(1000);

            //_view?.HideView();
            _view?.CloseView();

            //var loginView = _factory.Create<ILoginView, LoginPresenter>();

            //Form loginForm = loginView as Form;
            //if (loginForm == null)
            //    throw new InvalidOperationException("LoginView is not a Form");

            //loginForm.Show();
            //System.Windows.Forms.Application.Run((Form)loginView);
            //loginView.ShowView();

        }
    }
}
