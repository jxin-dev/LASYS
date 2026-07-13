using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Mappings;
using LASYS.Application.Common.Messaging;
using LASYS.Application.Common.Models;
using LASYS.Application.Common.Utilities;
using LASYS.Application.Contracts;
using LASYS.Application.Events;
using LASYS.Application.Features.BatchPrinting.Enums;
using LASYS.Application.Features.BatchPrinting.Events;
using LASYS.Application.Features.BatchPrinting.Helpers;
using LASYS.Application.Features.BatchPrinting.Models;
using LASYS.Application.Features.LabelInstructions.GetLabelInstructionContext;
using LASYS.Application.Features.OCRCalibration.GetOcrLabelFilePath;
using LASYS.Application.Features.OCRCalibration.PrintLabel;
using LASYS.Application.Features.OCRCalibration.PrintSampleLabel;
using LASYS.Application.Features.PrintLabels.Helpers;
using LASYS.Application.Interfaces.Services;
using LASYS.Application.Interfaces.Services.Camera;
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
        private readonly IDeviceManager _deviceManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMediator _mediator;
        private readonly INiceLabelTemplateService _niceLabelTemplateService;

        private readonly IFrameHub _frameHub;
        private Guid _cameraSubId;

        public VisionSettingsPresenter(IVisionSettingsView view, IDeviceManager deviceManager, IOCRService ocrService, ICalibrationService calibrationService, IServiceProvider serviceProvider, IMediator mediator, IFrameHub frameHub, INiceLabelTemplateService niceLabelTemplateService)
        {
            _view = view;
            View = (UserControl)view;

            _deviceManager = deviceManager;
            _ocrService = ocrService;
            _calibrationService = calibrationService;
            _serviceProvider = serviceProvider;
            _mediator = mediator;
            _frameHub = frameHub;
            _niceLabelTemplateService = niceLabelTemplateService;

            SubscribeCamera();

            _view.ComputeImageRegionRequested += OnComputeImageRegionRequested;
            _view.SaveCalibrationClicked += OnSaveCalibrationClicked;
            _view.LoadRegisteredOcrItemsRequested += OnLoadRegisteredOcrItemsRequested;
            OnLoadRegisteredOcrItemsRequested(this, EventArgs.Empty);


            _view.OCRTriggered += OnOCRTriggered;
            _view.OCRCalibrationPreview += OnOCRCalibraionPreview;

            _deviceManager.Camera.CameraNotification += OnCameraNotification;

            _ocrService.OCRRegionDetected += OnOCRRegionDetected;
            _ocrService.OCRRegionPreview += OnOCRRegionPreview;

            _view.FocusValueChanged += OnFocuseValueChanged;
            _view.LoadCamerasRequested += OnLoadCamerasRequested;
            _view.CameraResolutionSelected += OnCameraResolutionSelected;
            _view.CameraPreviewStateChanged += OnCameraPreviewStateChanged;
            _view.CameraConfigurationSaved += OnCameraConfigurationSaved;

            _view.SelectOcrItemRequested += OnSelectOcrItemRequested;
            _view.OcrItemChosen += OnOcrItemChosen;
            _view.PrintLabelRequested += OnPrintLabelRequested;
        }
        private readonly SemaphoreSlim _printSemaphore = new(1, 1);
        private async void OnPrintLabelRequested(object? sender, PrintLabelEventArgs e)
        {
            if (!await _printSemaphore.WaitAsync(0))
            {
                _view.ShowError("A sample label is already being generated.");
                return;
            }

            try
            {
                var result = await _mediator.Send(new PrintSampleLabelCommand(e.ItemCode, e.MasterRevision, e.BoxType, e.FilePath));
                if (!result.IsSuccess)
                {
                    _view.ShowError(result.Error!);
                    return;
                }

                var context = result.Value!;
                var labelInstruction = context.LabelInstructionDetails!;
                var masterLabel = context.MasterLabelDetails!;
                var niceLabelPath = masterLabel?.FilePath;
                var niceLabelFile = masterLabel?.LabelFile;

             
                
                var sw = Stopwatch.StartNew();

                await Task.Run(async () =>
                {
                    string filePath = NiceLabelFilePathBuilder.Build(labelInstruction.ItemCode, labelInstruction.MasterLabelRevNumber, masterLabel!.BoxType);
                    bool isNiceLabelExist = !string.IsNullOrWhiteSpace(niceLabelPath) && File.Exists(niceLabelPath);

                    if (isNiceLabelExist)
                        await NiceLabelFilePathBuilder.CopyFileAsync(niceLabelPath!, filePath);
                    else
                        await NiceLabelFilePathBuilder.CreateFileAsync(filePath, niceLabelFile!);

                    var isTemplateLoaded = _niceLabelTemplateService.IsTemplateLoaded;
                    if (!isTemplateLoaded)
                    {
                        _niceLabelTemplateService.LoadTemplate(filePath);
                    }
                });

                sw.Stop();
                Debug.WriteLine($"{DateTime.Now}-Template saved in {sw.ElapsedMilliseconds}ms");

                await PrintSampleLabelAsync(context);
            }
            catch (Exception ex)
            {
                _view.ShowError(ex.Message);
            }
            finally
            {
                _niceLabelTemplateService.CloseTemplate();
                _printSemaphore.Release();
            }
        }

        private async Task PrintSampleLabelAsync(LabelPrintingContext context)
        {
            try
            {
                var formattedCurrentSequence = SequenceFormatter.Format(1, 6);

                NiceLabelVariableCollection labelData = NiceLabelDataMappings.ToLabelData(context);
                labelData.Add("BOX_NO", formattedCurrentSequence);

                var templateVariables = _niceLabelTemplateService.GetTemplateVariables().ToHashSet(StringComparer.OrdinalIgnoreCase);

                var variablesToSet = labelData.Variables.Where(x => templateVariables.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);

                _niceLabelTemplateService.SetVariables(variablesToSet);

                var filename = PrintFileNameBuilder.BuildOCRSample(context.LabelInstructionDetails!.ItemCode);

                var sampleDirectory = PrintJobPathBuilder.CreateSampleDirectory(context.LabelInstructionDetails!.ItemCode);

                var isPreviewGenerated = _niceLabelTemplateService.GeneratePreview(sampleDirectory, filename);

                var imagePath = Path.Combine(sampleDirectory, $"{filename}.jpg");

                var isPrnGenerated = _niceLabelTemplateService.GeneratePrn(sampleDirectory, filename, out string prnPath);

                if (!isPrnGenerated || string.IsNullOrWhiteSpace(prnPath) || !File.Exists(prnPath))
                {
                    _view.ShowError("The generated PRN file could not be found.");
                    return;
                }

                var isPrinted = await _deviceManager.Printer.IsPrinted(prnPath);
                if (!isPrinted)
                {
                    _view.ShowError("The printer failed to print the label. Please check the printer and try again.");
                }

            }
            catch (Exception ex)
            {
                _view.ShowError(ex.Message);
            }
        }

        private void UnsubscribeCamera()
        {
            _frameHub.Unsubscribe(_cameraSubId);
        }
        private void SubscribeCamera()
        {
            _cameraSubId = _frameHub.Subscribe(frame =>
            {
                _view.InvokeOnUI(() =>
                {
                    _view.DisplayFrame(frame);
                });

                frame.Dispose();
            });
        }



        private async void OnOcrItemChosen(Product product)
        {
            var result = await _mediator.Send(new GetOcrLabelFilePathQuery(
                product.ItemCode, (uint)product.RevisionNo, product.BoxType));

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

            _view.SetSelectedOcrItem(product.ItemCode, (uint)product.RevisionNo, product.BoxType, product.Coordinates);

        }

        private void OnSelectOcrItemRequested(object? sender, EventArgs e)
        {
            if (_printSemaphore.CurrentCount == 0)
            {
                _view.ShowError("A sample label is already being generated.");
                return;
            }
            var ocrItemLookupPresenter = _serviceProvider.GetRequiredService<OcrItemLookupPresenter>();
            var selected = ocrItemLookupPresenter.Show();
            if (selected.ItemCode != null)
            {
                _view.InvokeOnUI(() => _view.SetSelectedOcrItem(selected.ItemCode, selected.MasterLabelRevNumber, selected.BoxType, null));
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
            _deviceManager.Camera.SetResolution(e);
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

            var cameraResolutions = _deviceManager.Camera.GetCameraResolutions();

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

                _deviceManager.Camera.SaveCameraConfigAsync(config);

            }
        }

        private async void OnCameraPreviewStateChanged(object? sender, CameraSelectedEventArgs e)
        {
            try
            {

                if (!_deviceManager.Camera.IsCameraReady())
                {
                    await _deviceManager.Camera.ReconnectAsync();
                }

                //await _deviceManager.Camera.PreviewCameraAsync(e.CameraName);
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
            var config = await _deviceManager.Camera.LoadCameraConfigAsync();
            var cameras = _deviceManager.Camera.GetCameras();
            _view.InvokeOnUI(() =>
            {
                if (cameras != null && cameras.Any())
                {
                    var cameraResolution = _deviceManager.Camera.GetCameraResolutions().Keys.ToList();
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
            _deviceManager.Camera.SetFocus(e);
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

            //if (e.Revision <= 0)
            //{
            //    _view.FinishCalibration(
            //        "Please search and select a valid revision.",
            //        isError: true
            //    );
            //    return;
            //}

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
                _view.InvokeOnUI(() => _view.ClearOCRResult());
                var summary = new ConcurrentDictionary<string, int>();
                int counter = 0;

                for (int i = 0; i < 50; i++)
                {
                    token.ThrowIfCancellationRequested();

                    var snapshot = _deviceManager.Camera.GetSnapshot();
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
    }
}
