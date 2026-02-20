using LASYS.Camera.Interfaces;
using LASYS.DesktopApp.Events;
using LASYS.DesktopApp.Views.Interfaces;
using LASYS.OCR.Interfaces;
using OpenCvSharp;
using DrawingSize = System.Drawing.Size;

namespace LASYS.DesktopApp.Presenters
{
    public class OCRCalibrationPresenter
    {
        public UserControl View { get; }
        private readonly IOCRCalibrationView _view;
        private readonly IOCRService _ocrService;
        private readonly ICalibrationService _calibrationService;
        private readonly ICameraService _cameraService;
        public OCRCalibrationPresenter(IOCRCalibrationView view, ICameraService cameraService, IOCRService ocrService, ICalibrationService calibrationService)
        {
            _view = view;
            View = (UserControl)view;

            _cameraService = cameraService;
            _ocrService = ocrService;
            _calibrationService = calibrationService;


            _view.InitializeRequested += OnInitializeRequested;
            _view.StreamingRequested += OnStreamingRequested;

            _view.ComputeImageRegionRequested += OnComputeImageRegionRequested;
            _view.SaveCalibrationClicked += OnSaveCalibrationClicked;
            _view.LoadRegisteredOcrItemsRequested += OnLoadRegisteredOcrItemsRequested;
            OnLoadRegisteredOcrItemsRequested(this, EventArgs.Empty);


            _view.OCRTriggered += OnOCRTriggered;

            _cameraService.CameraDisconnected += OnCameraDeviceDisconnected;
            _cameraService.CameraConnected += OnCameraDeviceConnected;


            _cameraService.CameraStatusChanged += (sender, e) =>
            {
                _view?.InvokeOnUI(() =>
                {
                    _view.ShowCameraStatus(e.StatusMessage, e.IsError);
                });
            };

            _ocrService.OCRRegionDetected += (sender, e) =>
            {
                var region = e.Region;
                _view?.InvokeOnUI(() => _view.ShowOCRRegion(region));
            };

            _ocrService.OCRCompleted += (sender, e) =>
            {
                _view?.InvokeOnUI(() => _view.ShowOCRResult(e.Result, e.Message, e.Success));
            };
        }

        private async void OnSaveCalibrationClicked(object? sender, CalibrationEventArgs e)
        {
            if (e.ImageSize.IsEmpty)
                return;

            string message = "Something went wrong. Please try again.";
            bool isError = true;

            // 1️⃣ Validate ItemCode
            if (string.IsNullOrWhiteSpace(e.ItemCode))
            {
                _view.FinishCalibration("Item code cannot be empty. Please enter a valid item code.", isError: true);
                return;
            }

            try
            {
                var result = _calibrationService.ComputeImageRegion(
                e.Roi,
                e.PreviewSize,
                e.ImageSize);

                if (result == null)
                    throw new InvalidOperationException("Computed image region is null.");

                await _calibrationService.AddOrUpdateAsync(result.ImageRegion, e.ImageSize, e.ItemCode);
                message = "Coordinates were saved successfully.";
                isError = false;
            }
            catch (InvalidOperationException oex)
            {
                message = oex.Message;
            }
            catch (Exception ex)
            {
                message = $"An error occurred while saving the coordinates.\n\nDetails:\n{ex.Message}";
            }

            finally
            {
                _view.FinishCalibration(message, isError);
                OnLoadRegisteredOcrItemsRequested(this, EventArgs.Empty);
            }
        }

        private void OnComputeImageRegionRequested(object? sender, ImageRegionEventArgs e)
        {
            if (e.ImageSize.IsEmpty)
                return;

            var result = _calibrationService.ComputeImageRegion(
                e.Roi,
                e.PreviewSize,
                e.ImageSize);

            if (result == null)
                return;

            _view.InvokeOnUI(() => _view.UpdateCoordinateFields(result.ImageRegion, e.ImageSize));

        }


        private async void OnOCRTriggered(object? sender, OCRCoordinatesEventArgs e)
        {
            await _ocrService.ReadTextAsync(_cameraService.LastCapturedFrame!, _view.PictureBoxSize, e.X, e.Y, e.Width, e.Height, e.ImageWidth, e.ImageHeight);
        }

        private async void OnLoadRegisteredOcrItemsRequested(object? sender, EventArgs e)
        {
            var config = await _calibrationService.LoadAsync();
            _view?.InvokeOnUI(() => _view.DisplayRegisteredOcrItems(config));
        }
        private void OnCameraDeviceConnected(object? sender, EventArgs e)
        {
            _view?.InvokeOnUI(() =>
            {
                _view.SetReconnectCameraButtonVisibility(false);
            });
        }
        private void OnCameraDeviceDisconnected(object? sender, EventArgs e)
        {
            _view?.InvokeOnUI(() =>
            {
                _view.SetReconnectCameraButtonVisibility(true);
            });
        }

        private DrawingSize GetSafePictureBoxSize()
        {
            var size = _view.PictureBoxSize;
            return (size.Width > 0 && size.Height > 0) ? size : new System.Drawing.Size(640, 480); // fallback resolution
        }

        private void HandleFrame(Mat mat, Bitmap bitmap)
        {
            if (bitmap == null) return;
            Console.WriteLine($"Frame: {bitmap.Width}x{bitmap.Height}");
            _view.InvokeOnUI(() =>
            {
                _view.DisplayFrame(bitmap);
            });
            bitmap.Dispose(); // free temporary bitmap

        }
        private async void OnStreamingRequested(object? sender, EventArgs e)
        {
            try
            {
                await _cameraService.StartStreamingAsync(
                    HandleFrame,
                    GetSafePictureBoxSize);
            }
            catch (OperationCanceledException)
            {
                // expected on shutdown
            }
            catch (Exception ex)
            {
                // log or show error
            }
        }
        private async void OnInitializeRequested(object? sender, EventArgs e)
        {
            try
            {
                await _cameraService.InitializeAsync();
                await _cameraService.StartStreamingAsync(
                    HandleFrame,
                    GetSafePictureBoxSize);
            }
            catch (OperationCanceledException)
            {
                // expected on shutdown
            }
            catch (Exception ex)
            {
                // log or show error
            }

        }
    }
}
