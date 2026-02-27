using System.Collections.Concurrent;
using System.Diagnostics;
using LASYS.Application.Events;
using LASYS.Application.Interfaces;
using LASYS.DesktopApp.Events;
using LASYS.DesktopApp.Views.Interfaces;
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
        private readonly IPrinterService _printerService;
        private readonly IBarcodeService _barcodeService;

        public OCRCalibrationPresenter(IOCRCalibrationView view, ICameraService cameraService, IOCRService ocrService, ICalibrationService calibrationService, IPrinterService printerService, IBarcodeService barcodeService)
        {
            _view = view;
            View = (UserControl)view;

            _cameraService = cameraService;
            _ocrService = ocrService;
            _calibrationService = calibrationService;
            _printerService = printerService;
            _barcodeService = barcodeService;


            _view.ReconnectCameraRequested += OnReconnectCameraRequested;
            _view.InitializeRequested += OnInitializeRequested;

            _view.ComputeImageRegionRequested += OnComputeImageRegionRequested;
            _view.SaveCalibrationClicked += OnSaveCalibrationClicked;
            _view.LoadRegisteredOcrItemsRequested += OnLoadRegisteredOcrItemsRequested;
            OnLoadRegisteredOcrItemsRequested(this, EventArgs.Empty);


            _view.OCRTriggered += OnOCRTriggered;

            _cameraService.CameraDisconnected += OnCameraDeviceDisconnected;
            _cameraService.CameraConnected += OnCameraDeviceConnected;
            _cameraService.CameraStatusChanged += OnCameraStatusChanged;

            _ocrService.OCRRegionDetected += OnOCRRegionDetected;
            _ocrService.OCRCompleted += OnOCRCompleted;

            _printerService.PrinterStateChanged += OnPrinterStateChanged;
            _barcodeService.BarcodeStatusChanged += OnBarcodeStatusChanged;

        }

        private void OnBarcodeStatusChanged(object? sender, BarcodeStatusEventArgs e)
        {
            _view.InvokeOnUI(() => _view.ShowBarcodeStatus(e.Message, e.IsError));
        }

        private void OnPrinterStateChanged(object? sender, PrinterStateChangedEventArgs e)
        {
            _view.InvokeOnUI(() => _view.ShowPrinterStatus(e.Message));
        }

        private void OnOCRCompleted(object? sender, OCRCompletedEventArgs e)
        {
            _view.InvokeOnUI(() => _view.ShowOCRResult(e.Result, e.Message, e.Success));
        }

        private void OnOCRRegionDetected(object? sender, OCRRegionEventArgs e)
        {
            var region = e.Region;
            _view?.InvokeOnUI(() => _view.ShowOCRRegion(region));
        }

        private void OnCameraStatusChanged(object? sender, CameraStatusEventArgs e)
        {
            _view.InvokeOnUI(() => _view.ShowCameraStatus(e.StatusMessage, e.IsError));
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
            ConcurrentBag<string> ocrResults = new();
            int ocrCounter = 0;


            var tasks = new List<Task>();

            for (int i = 0; i < 50; i++)
            {
                var snapshot = _cameraService.GetSnapshot();
                if (snapshot == null)
                    continue;

                tasks.Add(Task.Run(async () =>
                {
                    var result = await _ocrService.ReadTextAsync(
                        snapshot,
                        _view.PictureBoxSize,
                        e.X, e.Y, e.Width, e.Height,
                        e.ImageWidth, e.ImageHeight);

                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        ocrResults.Add(result);
                        var count = Interlocked.Increment(ref ocrCounter);
                        Debug.WriteLine($"[{count}] {result}");
                    }
                }));

                await Task.Delay(1000); // 👈 delay between triggers
            }

            await Task.WhenAll(tasks);
            Debug.WriteLine($"Parallel stress complete. Count: {ocrResults.Count}");


            //var tasks = Enumerable.Range(0, 50).Select(async _ =>
            //{
            //    var snapshot = _cameraService.GetSnapshot();
            //    if (snapshot == null) return;

            //   var result = await _ocrService.ReadTextAsync(snapshot, _view.PictureBoxSize, e.X, e.Y, e.Width, e.Height, e.ImageWidth, e.ImageHeight);
            //    if (!string.IsNullOrWhiteSpace(result))
            //    {
            //        _ocrResults.Add(result);
            //        var count = Interlocked.Increment(ref _ocrCounter);

            //        Debug.WriteLine(
            //            $"[{count}] {DateTime.Now:HH:mm:ss.fff} | OCR: {result}");
            //        await Task.Delay(500);
            //    }
            //});
            //await Task.WhenAll(tasks);
            //Debug.WriteLine($"Parallel stress complete. Count: {ocrResults.Count}");


            //var snapshot = _cameraService.GetSnapshot();
            //if (snapshot == null) return;

            //await _ocrService.ReadTextAsync(snapshot, _view.PictureBoxSize, e.X, e.Y, e.Width, e.Height, e.ImageWidth, e.ImageHeight);
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
        private async void OnInitializeRequested(object? sender, EventArgs e)
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


                await _printerService.InitializeAsync();
                await _barcodeService.InitializeAsync();
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
        private async void OnReconnectCameraRequested(object? sender, EventArgs e)
        {
            try
            {
                await _cameraService.InitializeAsync();
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
    }
}
