using System.Threading.Tasks;
using LASYS.Camera.Interfaces;
using LASYS.Camera.Models;
using LASYS.Camera.Services;
using LASYS.DesktopApp.Events;
using LASYS.DesktopApp.Views.Interfaces;
using OpenCvSharp;

namespace LASYS.DesktopApp.Presenters
{
    public class WebCameraPresenter
    {
        public UserControl View { get; }

        private readonly IWebCameraView _view;
        private readonly ICameraConfig _cameraConfig;
        private readonly IPreviewCameraService _previewCameraService;
        private readonly ICameraEnumerator _cameraEnumerator;

        public event Action? ConfigurationSaved;
        public WebCameraPresenter(
            IWebCameraView view,
            ICameraConfig cameraConfig,
            ICameraService cameraService,
            ICameraEnumerator cameraEnumerator,
            IPreviewCameraService previewCameraService)
        {
            // Initialize the view
            View = (UserControl)view;

            _view = view;
            _cameraConfig = cameraConfig;
            _cameraEnumerator = cameraEnumerator;
            _previewCameraService = previewCameraService;


            _previewCameraService.FrameReady += OnFrameReady;

            _view.CameraPreviewStateChanged += OnCameraPreviewStateChanged;
            _view.CameraConfigurationSaved += OnCameraConfigurationSaved;
        }

        private void OnCameraConfigurationSaved(object? sender, CameraSavedEventArgs e)
        {
            var selectedName = e.CameraName;
            var cameraIndex = e.CameraIndex;

            var cameraResolutions = _cameraConfig.GetCameraResolutions();


            if (cameraResolutions.TryGetValue(e.Resolution, out var resolution))
            {
                int width = resolution.Width;
                int height = resolution.Height;

                Console.WriteLine($"Selected Camera: {selectedName} (Index {cameraIndex})");
                Console.WriteLine($"Resolution: {e.Resolution} => {width}x{height}");

                var config = new CameraConfig
                {
                    Index = cameraIndex,
                    Name = selectedName,
                    FrameWidth = width,
                    FrameHeight = height
                };
                _cameraConfig.SaveAsync(config);


                bool restart = _view.AskRestartConfirmation("Camera configuration saved. The application needs to restart to apply changes.\nDo you want to restart now?");
                
                if (restart)
                {
                    _cameraConfig.RestartApplication();
                }
            }
        }

        private void OnCameraPreviewStateChanged(object? sender, CameraSelectedEventArgs e)
        {
            try
            {
                _previewCameraService.StartCamera(e.CameraIndex);
                //_view.ShowMessage("Camera preview started.", "Camera Preview", MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "Camera Preview", MessageBoxIcon.Error);

            }
        }

        private void OnFrameReady(Mat mat)
        {
            Bitmap bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat);
            _view.InvokeOnUI(() => _view.DisplayFrame(bitmap));
        }

        public void StopCameraPreview()
        {
            try
            {
                _previewCameraService.StopCamera();
                _view.ShowMessage("Camera preview stopped.", "Camera Preview", MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "Camera Preview", MessageBoxIcon.Error);
            }
        }




        public void LoadCameras()
        {
            var cameras = _cameraEnumerator.GetCameras();

            if (cameras != null)
            {
                var cameraResolution = _cameraConfig.GetCameraResolutions().Keys.ToList();
                _view.SetCameraResolutions(cameraResolution);
                _view.SetCameraList(cameras);
            }
        }



    }
}
