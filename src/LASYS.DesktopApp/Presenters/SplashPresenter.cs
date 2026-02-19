using LASYS.Camera.Events;
using LASYS.Camera.Interfaces;
using LASYS.DesktopApp.Views.Interfaces;
using LASYS.SatoLabelPrinter.Interfaces;

namespace LASYS.DesktopApp.Presenters
{
    public class SplashPresenter
    {
        private ISplashView _view;
        private readonly ICameraConfig _cameraConfig;
        private readonly ICameraService _cameraService;
        private readonly IPrinterService _printerService;

        public SplashPresenter(ISplashView view, ICameraConfig cameraConfig, ICameraService cameraService, IPrinterService printerService)
        {
            _view = view;
            _view.ViewShown += OnViewShown;

            _cameraConfig = cameraConfig;
            _cameraService = cameraService;
            _printerService = printerService;

            _cameraConfig.CameraConfigIssue += OnCameraConfigIssue;
            _printerService.PrinterStateChanged += OnPrinterStateChanged;
        }

        private void OnPrinterStateChanged(object? sender, SatoLabelPrinter.Events.PrinterStateChangedEventArgs e)
        {
            _view.UpdateProgress(50, e.Message);
        }

        private async void OnViewShown(object? sender, EventArgs e)
        {
            await InitializeAsync();

        }
        private void OnCameraConfigIssue(object? sender, CameraConfigEventArgs e)
        {
            if (_view == null) return;
            // Ensure UI thread update if needed
            _view.UpdateProgress(15, e.Message);
        }

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


            await _printerService.InitializeAsync();// 50%

            _view?.UpdateProgress(80, "Finalizing setup...");
            await Task.Delay(2000);
            _view?.UpdateProgress(100, "Launching application...");
            await Task.Delay(1000);

            _view?.CloseView();

        }
    }
}
