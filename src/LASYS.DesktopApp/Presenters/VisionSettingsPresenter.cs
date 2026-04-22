using System.Collections.Concurrent;
using System.Diagnostics;
using LASYS.Application.Contracts;
using LASYS.Application.Events;
using LASYS.Application.Features.OCRCalibration.GetOcrLabelFilePath;
using LASYS.Application.Interfaces.Services;
using LASYS.DesktopApp.Events;
using LASYS.DesktopApp.Views.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OpenCvSharp;
using DrawingSize = System.Drawing.Size;

namespace LASYS.DesktopApp.Presenters
{
    public class VisionSettingsPresenter
    {
        public UserControl View { get; }
        private readonly IVisionSettingsView _view;
        private readonly IOCRService _ocrService;
        private readonly ICalibrationService _calibrationService;
        private readonly ICameraService _cameraService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMediator _mediator;

        public VisionSettingsPresenter(IVisionSettingsView view, ICameraService cameraService, IOCRService ocrService, ICalibrationService calibrationService, IServiceProvider serviceProvider, IMediator mediator)
        {
            _view = view;
            View = (UserControl)view;

            _cameraService = cameraService;
            _ocrService = ocrService;
            _calibrationService = calibrationService;
            _serviceProvider = serviceProvider;
            _mediator = mediator;


            _view.InitializeRequested += OnInitializeRequested;

            _view.ComputeImageRegionRequested += OnComputeImageRegionRequested;
            _view.SaveCalibrationClicked += OnSaveCalibrationClicked;
            _view.LoadRegisteredOcrItemsRequested += OnLoadRegisteredOcrItemsRequested;
            OnLoadRegisteredOcrItemsRequested(this, EventArgs.Empty);


            _view.OCRTriggered += OnOCRTriggered;
            _view.OCRCalibrationPreview += OnOCRCalibraionPreview;

            _cameraService.CameraNotification += OnCameraNotification;

            _ocrService.OCRRegionDetected += OnOCRRegionDetected;
            _ocrService.OCRRegionPreview += OnOCRRegionPreview;

            _view.FocusValueChanged += OnFocuseValueChanged;
            _view.LoadCamerasRequested += OnLoadCamerasRequested;
            _view.CameraResolutionSelected += OnCameraResolutionSelected;
            _view.CameraPreviewStateChanged += OnCameraPreviewStateChanged;
            _view.CameraConfigurationSaved += OnCameraConfigurationSaved;

            _view.SelectOcrItemRequested += OnSelectOcrItemRequested;
            _view.OcrItemChosen += OnOcrItemChosen;
        }

        private async void OnOcrItemChosen(Product product)
        {
            var result = await _mediator.Send(new GetOcrLabelFilePathQuery(
                product.ItemCode,(uint)product.RevisionNo,product.BoxType));

            if (!result.IsSuccess)
            {
                _view.ShowError(result.ErrorOrDefault ?? "No file path found.");
                return;
            }

            var filePath = result.Value;

            if (string.IsNullOrWhiteSpace(filePath))
            {
                _view.ShowError("Invalid file path.");
                return;
            }

            _view.SetSelectedOcrItem(new(product.ItemCode, (uint)product.RevisionNo, product.BoxType, filePath), product.Coordinates);

        }

        private void OnSelectOcrItemRequested(object? sender, EventArgs e)
        {
            var ocrItemLookupPresenter = _serviceProvider.GetRequiredService<OcrItemLookupPresenter>();
            var selected = ocrItemLookupPresenter.Show();
            if (selected != null)
            {
                _view.InvokeOnUI(() => _view.SetSelectedOcrItem(selected, null));
            }
        }

        private void OnOCRRegionPreview(object? sender, OCRRegionEventArgs e)
        {
            var region = e.Region;
            _view?.InvokeOnUI(() => _view.PreviewOCRRegion(region));
        }

        private void OnOCRCalibraionPreview(object? sender, OCRCoordinatesEventArgs e)
        {
            _ocrService.PreviewRegion(_view.PictureBoxSize, e.X, e.Y, e.Width, e.Height, e.ImageWidth, e.ImageHeight);

        }

        private void OnCameraResolutionSelected(object? sender, string e)
        {
            _cameraService.SetResolution(e);
        }

        private void OnCameraNotification(object? sender, CameraNotificationEventArgs e)
        {
            _view.ShowCameraNotification(e.Message, e.Caption, e.IsError);
        }

        private void OnCameraConfigurationSaved(object? sender, CameraSavedEventArgs e)
        {
            var cameraIndex = e.CameraIndex;
            var cameraName = e.CameraName;
            var resolutionKey = e.Resolution;
            var focusValue = e.Focus;

            var cameraResolutions = _cameraService.GetCameraResolutions();

            if (cameraResolutions.TryGetValue(resolutionKey, out var resolution))
            {
                int width = resolution.Width;
                int height = resolution.Height;

                Debug.WriteLine($"Selected Camera: {cameraName} (Index {cameraIndex})");
                Debug.WriteLine($"Resolution: {e.Resolution} => {width}x{height}");

                var config = new CameraConfig
                {
                    Index = cameraIndex,
                    Name = cameraName,
                    Resolution = resolutionKey,
                    Focus = focusValue
                };

                _cameraService.SaveCameraConfigAsync(config);

            }
        }

        private async void OnCameraPreviewStateChanged(object? sender, CameraSelectedEventArgs e)
        {
            try
            {
                await _cameraService.PreviewCameraAsync(e.CameraName);
                // Start streaming in background safely
                _ = _cameraService.StartStreamingAsync(
                    HandleFrame,
                    GetSafePictureBoxSize)
                    .ContinueWith(t =>
                    {
                        if (t.Exception != null)
                            Debug.WriteLine(t.Exception.Flatten());
                    }, TaskContinuationOptions.OnlyOnFaulted);
            }
            catch (OperationCanceledException)
            {
                // expected on shutdown
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private async void OnLoadCamerasRequested(object? sender, EventArgs e)
        {
            var config = await _cameraService.LoadCameraConfigAsync();
            var cameras = _cameraService.GetCameras();
            _view.InvokeOnUI(() =>
            {
                if (cameras != null && cameras.Any())
                {
                    var cameraResolution = _cameraService.GetCameraResolutions().Keys.ToList();
                    _view.SetCameraList(cameras);
                    _view.SetCameraResolutions(cameraResolution);

                    if (config != null)
                    {
                        _view.SelectCamera(config.Name, config.Resolution, config.Focus);
                    }
                }
                else
                {
                    _view.SetCameraResolutions(null);
                    _view.SetCameraList(null);
                }
            });
        }


        private void OnFocuseValueChanged(object? sender, int e)
        {
            _cameraService.SetFocus(e);
        }
        private void OnOCRRegionDetected(object? sender, OCRRegionEventArgs e)
        {
            var region = e.Region;
            _view?.InvokeOnUI(() => _view.ShowOCRRegion(region));
        }
        private async void OnSaveCalibrationClicked(object? sender, CalibrationEventArgs e)
        {
            if (e.ImageSize.IsEmpty)
                return;

            string message = "Something went wrong. Please try again.";
            bool isError = true;

            if (string.IsNullOrWhiteSpace(e.ItemCode))
            {
                _view.FinishCalibration(
                    "Please search and select an item first.",
                    isError: true
                );
                return;
            }

            if (e.Revision <= 0)
            {
                _view.FinishCalibration(
                    "Please search and select a valid revision.",
                    isError: true
                );
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

                await _calibrationService.AddOrUpdateAsync(result.ImageRegion, e.ImageSize, e.ItemCode, e.Revision, e.BoxType);
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

        private CancellationTokenSource? _ocrCts;

        private async Task RunOcrTest(OCRCoordinatesEventArgs e, CancellationToken token)
        {
            try
            {
                var summary = new ConcurrentDictionary<string, int>();
                int counter = 0;

                for (int i = 0; i < 50; i++)
                {
                    token.ThrowIfCancellationRequested();

                    var snapshot = _cameraService.GetSnapshot();
                    if (snapshot == null)
                        continue;

                    var result = await _ocrService.ReadTextAsync(
                        snapshot,
                        _view.PictureBoxSize,
                        e.X, e.Y, e.Width, e.Height,
                        e.ImageWidth, e.ImageHeight);

                    var count = Interlocked.Increment(ref counter);

                    var key = string.IsNullOrWhiteSpace(result)
                        ? "No text detected"
                        : result.Trim();

                    summary.AddOrUpdate(key, 1, (_, old) => old + 1);

                    _view.InvokeOnUI(() =>
                        _view.DisplayOCRResult($"[{count}] {key}")
                    );

                    await Task.Delay(1000, token); // cancellable delay
                }

                // SUMMARY OUTPUT
                _view.InvokeOnUI(() =>
                {
                    _view.DisplayOCRResult("----- SUMMARY -----");

                    foreach (var item in summary
                        .OrderByDescending(x => x.Value) 
                        .ThenBy(x => x.Key))             
                    {
                        _view.DisplayOCRResult($"{item.Key} → {item.Value}");
                    }

                    _view.DisplayOCRResult($"Total Runs: {counter}");
                });
            }
            catch (OperationCanceledException)
            {
                _view.InvokeOnUI(() =>
                    _view.DisplayOCRResult("OCR test cancelled."));
            }
            finally
            {
                _view.InvokeOnUI(() =>
                    _view.TestOCRTCompleted());
            }
        }
        private async void OnOCRTriggered(object? sender, OCRCoordinatesEventArgs e)
        {
            if (e.X == 0 || e.Y == 0 || e.Width == 0 || e.Height == 0 || e.ImageWidth == 0 || e.ImageHeight == 0)
            {
                _view.DisplayOCRResult("OCR failed: No coordinates selected.");
                return;
            }

            if (_ocrCts != null)
            {
                _ocrCts.Cancel();
                return;
            }

            _ocrCts = new CancellationTokenSource();
            _view.InvokeOnUI(() => _view.SetTestButtonText("Cancel OCR"));

            try
            {
                await RunOcrTest(e, _ocrCts.Token);
            }
            finally
            {
                _ocrCts = null;
                _view.SetTestButtonText("Run OCR");
            }

        }

        private async void OnLoadRegisteredOcrItemsRequested(object? sender, EventArgs e)
        {
            var config = await _calibrationService.LoadAsync();
            _view?.InvokeOnUI(() => _view.DisplayRegisteredOcrItems(config));
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
        private void OnInitializeRequested(object? sender, EventArgs e)
        {
            try
            {
                // Start camera streaming in the background
                _ = _cameraService.StartStreamingAsync(
                    HandleFrame,
                    GetSafePictureBoxSize)
                    .ContinueWith(t =>
                    {
                        if (t.Exception != null)
                            Debug.WriteLine(t.Exception.Flatten());
                    }, TaskContinuationOptions.OnlyOnFaulted);

            }
            catch (OperationCanceledException)
            {
                // expected on shutdown
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

    }
}
