using LASYS.Camera.Interfaces;
using LASYS.Camera.Models;
using LASYS.DesktopApp.Views.Interfaces;

namespace LASYS.DesktopApp.Presenters
{
    public class WebCameraPresenter
    {
        public UserControl View { get; }

        private readonly IWebCameraView _view;
        private readonly ICameraConfig _cameraConfig;
        private readonly ICameraService _cameraService;
        private readonly ICameraEnumerator _cameraEnumerator;

        private CancellationTokenSource? _previewCts;

        public event Action? ConfigurationSaved;
        public WebCameraPresenter(
            IWebCameraView view,
            ICameraConfig cameraConfig,
            ICameraService cameraService,
            ICameraEnumerator cameraEnumerator)
        {
            // Initialize the view
            View = (UserControl)view;

            _view = view;
            _cameraConfig = cameraConfig;
            _cameraService = cameraService;
            _cameraEnumerator = cameraEnumerator;


            _view.CameraPreviewStateChanged += OnCameraPreviewStateChanged;
            _view.CameraConfigurationSaved += OnCameraConfigurationSaved;

        }
        public void LoadCameras()
        {
            var cameras = _cameraEnumerator.GetCameras();
            _view.SetCameraList(cameras);
        }
        private async void OnCameraConfigurationSaved(object? sender, EventArgs e)
        {
            var selectedCamera = _view.SelectedCamera;
            if (selectedCamera is null)
            {
                _view.ShowMessage(
                    "Please select a camera before saving.",
                    "Save Configuration",
                    MessageBoxIcon.Warning);
                return;
            }
            var newCamera = new CameraConfig
            {
                Index = selectedCamera.Index,
                Name = selectedCamera.Name,
                FrameWidth = 3840,
                FrameHeight = 2160,
                FrameRate = 30
            };
            try
            {
                await _cameraConfig.SaveAsync(newCamera);
                ConfigurationSaved?.Invoke();
            }
            catch (Exception ex)
            {
                _view.ShowMessage($"Failed to save configuration.\n{ex.Message}", "Error", MessageBoxIcon.Error);
            }
          
        }

        private void OnCameraPreviewStateChanged(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

    }
}
