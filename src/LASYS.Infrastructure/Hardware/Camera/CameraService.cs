using System.Diagnostics;
using System.Drawing;
using DirectShowLib;
using LASYS.Application.Common.Enums;
using LASYS.Application.Contracts;
using LASYS.Application.Events;
using LASYS.Application.Interfaces.Services;
using Newtonsoft.Json;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using DrawingSize = System.Drawing.Size;

namespace LASYS.Infrastructure.Hardware.Camera
{
    public class CameraService : ICameraService
    {
        private readonly string _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "camera.config.json");
        public event EventHandler<CameraConfigEventArgs>? CameraConfigIssue;

        public event EventHandler<CameraStatusEventArgs>? CameraStatusChanged;
        public event EventHandler? CameraDisconnected;
        public event EventHandler? CameraConnected;
        public event EventHandler<CameraNotificationEventArgs>? CameraNotification;

        private VideoCapture? _capture;
        private CameraConfig? _activeConfig;

        private bool _hasReportedEmptyFrame;
        private bool _disposed;


        private bool _isCameraConnected;
        private Task? _streamingTask;
        public Mat? LastCapturedFrame { get; set; }

        private CancellationTokenSource _cts;
        private readonly object _captureLock = new();
        private readonly object _frameLock = new();

        private Bitmap? _lastBitmap;

        private bool _isStreaming = false;
        public bool IsStreaming => _isStreaming;

        public CameraService()
        {
            _cts = new CancellationTokenSource();
        }


        public async Task InitializeAsync()
        {
            await StopAsync();
            _activeConfig = await LoadCameraConfigAsync();
            await OpenCameraAsync(_activeConfig);
        }

        // ----------------------------------------------------
        // Camera resolution
        // ----------------------------------------------------
        //public CameraInfo? ResolveCamera(CameraConfig config)
        //{
        //    var cameras = GetCameras();

        //    return cameras.FirstOrDefault(c =>
        //               c.Index == config.Index &&
        //               string.Equals(c.Name, config.Name, StringComparison.OrdinalIgnoreCase))
        //           ?? cameras.FirstOrDefault(c =>
        //               string.Equals(c.Name, config.Name, StringComparison.OrdinalIgnoreCase));
        //}


        private async Task OpenCameraAsync(CameraConfig config)
        {
            ReleaseCamera();

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));
            var resolutions = GetCameraResolutions();
            try
            {
                await Task.Run(() =>
                {
                    var cameraIndex = GetCameraIndex(config.Name);
                    if (cameraIndex == -1)
                    {
                        if (string.IsNullOrWhiteSpace(config.Name))
                            CameraStatusChanged?.Invoke(this, new CameraStatusEventArgs(CameraStatus.CameraNotConfigured));
                        else
                            CameraStatusChanged?.Invoke(this, new CameraStatusEventArgs(CameraStatus.CameraNotDetected, $"No camera device was found using the configured device name ({config.Name})"));

                        CameraDisconnected?.Invoke(this, EventArgs.Empty);
                        return;
                    }

                    //var cameraInfo = ResolveCamera(config);
                    //if (cameraInfo == null)
                    //{
                    //    CameraStatusChanged?.Invoke(this, new CameraStatusEventArgs(
                    //            $"Device unavailable ({config.Name})", true));
                    //    CameraDisconnected?.Invoke(this, EventArgs.Empty);
                    //    return;
                    //}
                    //var cameraIndex = cameraInfo.Index;

                    //var capture = new VideoCapture(cameraIndex);
                    var capture = new VideoCapture(cameraIndex, VideoCaptureAPIs.DSHOW);


                    if (!capture.IsOpened())
                    {
                        return; // hot-plug safe
                    }

                    if (resolutions.TryGetValue(config.Resolution, out var resolution))
                    {
                        capture.Set(VideoCaptureProperties.FrameWidth, resolution.Width);
                        capture.Set(VideoCaptureProperties.FrameHeight, resolution.Height);
                    }
                    capture.Set(VideoCaptureProperties.Fps, 10);

                    if (config.Focus > 0)
                    {
                        //capture.Set(VideoCaptureProperties.AutoExposure, 0);
                        //capture.Set(VideoCaptureProperties.Exposure, -5);
                        //capture.Set(VideoCaptureProperties.AutoWB, 0);
                        //capture.Set(VideoCaptureProperties.WBTemperature, 5500);
                        capture.Set(VideoCaptureProperties.AutoFocus, 0);
                        capture.Set(VideoCaptureProperties.Focus, config.Focus);
                    }
                    else
                    {
                        capture.Set(VideoCaptureProperties.AutoFocus, 1);
                    }

                    var focus = capture.Get(VideoCaptureProperties.Focus);

                    _capture = capture;

                }, timeoutCts.Token);


                _hasReportedEmptyFrame = false;
                _isCameraConnected = false;
            }
            catch (OperationCanceledException)
            {
                if (!_cts.IsCancellationRequested)
                {
                    CameraStatusChanged?.Invoke(this, new CameraStatusEventArgs(CameraStatus.CameraTimeout));
                    CameraDisconnected?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception)
            {
                CameraStatusChanged?.Invoke(this, new CameraStatusEventArgs(CameraStatus.CameraError));
            }

        }

        // ----------------------------------------------------
        // Streaming
        // ----------------------------------------------------

        public Task StartStreamingAsync(
            Action<Mat, Bitmap> onFrameCaptured,
            Func<DrawingSize> getTargetResolution)
        {

            lock (_captureLock)
            {
                if (_isStreaming)
                    return Task.CompletedTask; // Already streaming

                if (_capture == null || !_capture.IsOpened())
                {
                    CameraStatusChanged?.Invoke(this, new CameraStatusEventArgs(CameraStatus.CameraDisconnected));
                    CameraDisconnected?.Invoke(this, EventArgs.Empty);
                    return Task.CompletedTask;
                }

                _isStreaming = true;
                _cts ??= new CancellationTokenSource();
            }


            _isCameraConnected = false;

            var token = _cts.Token;

            _streamingTask = Task.Factory.StartNew(() =>
            {
                var frameInterval = TimeSpan.FromMilliseconds(100);
                var lastUpdate = DateTime.UtcNow;

                while (!token.IsCancellationRequested)
                {
                    if (!IsCameraReady())
                    {
                        CreateAndCaptureEmptyFrame(getTargetResolution(), onFrameCaptured);

                        CameraStatusChanged?.Invoke(this, new CameraStatusEventArgs(CameraStatus.CameraDisconnected));

                        CameraDisconnected?.Invoke(this, EventArgs.Empty);

                        Thread.Sleep(1000);
                        continue;
                    }

                    using var frame = new Mat();
                    using var resized = new Mat();

                    if (!TryReadFrame(frame) || frame.Empty())
                    {
                        HandleEmptyFrame(onFrameCaptured, getTargetResolution);

                        CameraStatusChanged?.Invoke(this, new CameraStatusEventArgs(CameraStatus.CameraDisconnected));

                        CameraDisconnected?.Invoke(this, EventArgs.Empty);
                        continue;
                    }

                    var elapsed = DateTime.UtcNow - lastUpdate;
                    if (elapsed < frameInterval)
                        Thread.Sleep(frameInterval - elapsed);

                    lastUpdate = DateTime.UtcNow;

                    var targetSize = getTargetResolution();
                    Cv2.Resize(frame, resized, new OpenCvSharp.Size(targetSize.Width, targetSize.Height));

                    HandleFrameSafe(resized, onFrameCaptured);

                    lock (_frameLock)
                    {
                        //LastCapturedFrame?.Dispose();
                        //LastCapturedFrame = resized.Clone();
                        if (LastCapturedFrame == null)
                        {
                            LastCapturedFrame = resized.Clone();
                        }
                        else
                        {
                            CameraStatusChanged?.Invoke(this, new CameraStatusEventArgs(CameraStatus.CameraConnected));
                            resized.CopyTo(LastCapturedFrame);

                            //var focus = _capture?.Get(VideoCaptureProperties.Focus);
                            //Debug.WriteLine($"Focus: {focus}");

                            //double actualWidth = _capture.Get(VideoCaptureProperties.FrameWidth);
                            //double actualHeight = _capture.Get(VideoCaptureProperties.FrameHeight);
                            //Debug.WriteLine($"Camera Resolution: {actualWidth}x{actualHeight}");
                        }
                    }

                    ReportConnectedOnce();
                }

            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);


            return _streamingTask;
        }
        private void HandleFrameSafe(Mat resized, Action<Mat, Bitmap> onFrameCaptured)
        {
            var bitmap = resized.ToBitmap();
            onFrameCaptured(resized, bitmap);

            //if (_capture != null)
            //    Debug.Write($"FH:{_capture.FrameHeight} | FW:{_capture.FrameWidth} | FPS: {_capture.Fps}\n");
        }


        // ----------------------------------------------------
        // Helpers
        // ----------------------------------------------------
        private bool TryReadFrame(Mat frame)
        {
            lock (_captureLock)
            {
                try
                {
                    if (_capture == null || !_capture.IsOpened() || _capture.IsDisposed)
                        return false;

                    return _capture.Read(frame);
                }
                catch (AccessViolationException)
                {
                    // The camera was unplugged
                    ReleaseCamera();

                    //_capture?.Dispose();
                    //_capture = null;
                    return false;
                }
                catch (Exception)
                {
                    // Other errors
                    return false;
                }
            }
        }


        public bool IsCameraReady() =>
            _capture != null && _capture.IsOpened() && !_capture.IsDisposed;

        private async Task HandleDisconnectedCamera(
            Action<Mat, Bitmap> onFrameCaptured,
            Func<DrawingSize> getTargetResolution)
        {
            try
            {
                CreateAndCaptureEmptyFrame(getTargetResolution(), onFrameCaptured);

                await Task.Delay(1000, _cts.Token);

                if (_activeConfig != null && !_cts.Token.IsCancellationRequested)
                {
                    //await OpenCameraAsync(_activeConfig);

                    if (IsCameraReady())
                    {
                        await OpenCameraAsync(_activeConfig);
                        _hasReportedEmptyFrame = false;
                        _isCameraConnected = false;
                        _streamingTask = null; // Restart streaming
                    }
                    else
                    {
                        CameraStatusChanged?.Invoke(this, new CameraStatusEventArgs(CameraStatus.CameraDisconnected));
                        CameraDisconnected?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HandleDisconnectedCamera failed: {ex}");
            }


        }

        protected virtual void OnCameraDisconnected()
        {
            var handler = CameraDisconnected;
            if (handler == null) return;

            foreach (EventHandler subscriber in handler.GetInvocationList())
            {
                try
                {
                    subscriber.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"CameraDisconnected subscriber failed: {ex}");
                }
            }
        }

        private void HandleEmptyFrame(
            Action<Mat, Bitmap> onFrameCaptured,
            Func<DrawingSize> getTargetResolution)
        {
            if (_hasReportedEmptyFrame)
                return;

            _hasReportedEmptyFrame = true;
            _isCameraConnected = false;

            //CameraStatusChanged?.Invoke(this,
            //    new CameraStatusEventArgs(
            //        "No image received from the camera.", true));
            //CameraDisconnected?.Invoke(this, EventArgs.Empty);

            CreateAndCaptureEmptyFrame(getTargetResolution(), onFrameCaptured);
        }

        private static void Throttle(ref DateTime lastUpdate, TimeSpan interval)
        {
            var now = DateTime.Now;
            if (now - lastUpdate < interval)
                return;

            lastUpdate = now;
        }

        private void ReportConnectedOnce()
        {
            if (_isCameraConnected)
                return;

            CameraConnected?.Invoke(this, EventArgs.Empty);

            CameraStatusChanged?.Invoke(this, new CameraStatusEventArgs(CameraStatus.CameraConnected));

            _isCameraConnected = true;
        }

        private void CreateAndCaptureEmptyFrame(DrawingSize size, Action<Mat, Bitmap> onFrameCaptured)
        {
            using var mat = new Mat(size.Height, size.Width, MatType.CV_8UC3, Scalar.All(0));
            using var bitmap = mat.ToBitmap();

            _lastBitmap?.Dispose();
            _lastBitmap = new Bitmap(bitmap);

            onFrameCaptured(mat, _lastBitmap);

            ReleaseCamera();
        }



        // ----------------------------------------------------
        // Cleanup
        // ----------------------------------------------------
        public void ReleaseCamera()
        {
            lock (_captureLock)
            {
                try { _capture?.Release(); } catch { }
                try { _capture?.Dispose(); } catch { }
                _capture = null;
            }

        }
        public async Task StopAsync()
        {
            lock (_captureLock)
            {
                if (!_isStreaming)
                    return;

                _cts?.Cancel();
            }

            try
            {
                if (_streamingTask != null)
                    await _streamingTask;
            }
            catch (OperationCanceledException)
            {
                // expected
            }
            finally
            {
                lock (_captureLock)
                {
                    _isStreaming = false;
                    _cts?.Dispose();
                    _cts = new CancellationTokenSource();
                    _streamingTask = null;
                    _hasReportedEmptyFrame = false;
                    _isCameraConnected = false;
                }
                LastCapturedFrame?.Dispose();
                LastCapturedFrame = null;

                _lastBitmap?.Dispose();
                _lastBitmap = null;

                ReleaseCamera();
            }
        }


        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            ReleaseCamera();
            LastCapturedFrame?.Dispose();
        }

        public Mat? GetSnapshot()
        {
            lock (_frameLock)
            {
                if (LastCapturedFrame == null || LastCapturedFrame.Empty())
                    return null;
                CameraStatusChanged?.Invoke(this, new CameraStatusEventArgs(CameraStatus.CameraCapturing));
                return LastCapturedFrame.Clone();  // safe snapshot
            }
        }

        public void SetFocus(int focusValue)
        {
            if (_capture == null || !_capture.IsOpened() || _capture.IsDisposed) return;

            if (focusValue == 0)
            {
                _capture.Set(VideoCaptureProperties.AutoFocus, 1);
            }
            else
            {
                _capture.Set(VideoCaptureProperties.AutoFocus, 0);
                _capture.Set(VideoCaptureProperties.Focus, focusValue);
            }

            var focus = _capture?.Get(VideoCaptureProperties.Focus);
            //Debug.WriteLine($"Focus: {focus}");
            CameraStatusChanged?.Invoke(this, new CameraStatusEventArgs(
                CameraStatus.CameraFocusing,
                focusValue == 0
                ? "Auto focus is being enabled."
                : $"Manual focus is being set to value {focusValue}."));

        }

        public int GetCameraIndex(string cameraName)
        {
            var devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            for (int i = 0; i < devices.Length; i++)
            {
                if (string.Equals(devices[i].Name, cameraName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1; // camera not found
        }

        public IReadOnlyList<string> GetCameras()
        {
            return DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice).Select(d => d.Name).ToList();
        }

        public async Task<CameraConfig> LoadCameraConfigAsync()
        {
            try
            {
                if (!File.Exists(_configPath))
                {
                    CameraStatusChanged?.Invoke(this, new CameraStatusEventArgs(CameraStatus.CameraNotConfigured));
                    await Task.Delay(1000);
                    return new CameraConfig();
                }

                var json = await File.ReadAllTextAsync(_configPath);
                return JsonConvert.DeserializeObject<CameraConfig>(json)
                       ?? new CameraConfig();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to load camera.config.json: {ex.Message}");
                CameraStatusChanged?.Invoke(this, new CameraStatusEventArgs(CameraStatus.CameraError));
                await Task.Delay(1000);
                return new CameraConfig();
            }
        }

        public async Task SaveCameraConfigAsync(CameraConfig config)
        {
            try
            {
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                await File.WriteAllTextAsync(_configPath, json);
                CameraNotification?.Invoke(this, new CameraNotificationEventArgs("Camera configuration saved successfully.", "Configuration"));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to save config: {ex.Message}");
                CameraNotification?.Invoke(this, new CameraNotificationEventArgs("Failed to save camera.config.json", "Configuration", true));
                throw;
            }
        }

        public Dictionary<string, Resolution> GetCameraResolutions()
        {
            return new Dictionary<string, Resolution>
            {
                ["HD / 720p"] = new Resolution
                {
                    Width = 1280,
                    Height = 720,
                    AspectRatio = "16:9",
                    Notes = "Standard high definition"
                },
                ["Full HD / 1080p"] = new Resolution
                {
                    Width = 1920,
                    Height = 1080,
                    AspectRatio = "16:9",
                    Notes = "Most webcams, monitors, streaming"
                },
                ["2K / 1440p"] = new Resolution
                {
                    Width = 2560,
                    Height = 1440,
                    AspectRatio = "16:9",
                    Notes = "High detail, heavier processing"
                },
                ["4K UHD / 2160p"] = new Resolution
                {
                    Width = 3840,
                    Height = 2160,
                    AspectRatio = "16:9",
                    Notes = "Ultra HD"
                }
            };
        }

        public void RestartApplication()
        {
            string exePath = Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;
            if (string.IsNullOrEmpty(exePath))
            {
                Console.Error.WriteLine("Unable to determine executable path for restart.");
                return;
            }
            try
            {
                Process.Start(exePath);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to restart application: {ex.Message}");
                CameraStatusChanged?.Invoke(this, new CameraStatusEventArgs(CameraStatus.CameraError));
            }
        }

        public void SetResolution(string resolutionKey)
        {
            if (_capture == null || !_capture.IsOpened() || _capture.IsDisposed) return;

            var resolutions = GetCameraResolutions();
            if (resolutions.TryGetValue(resolutionKey, out var resolution))
            {
                _capture.Set(VideoCaptureProperties.FrameWidth, resolution.Width);
                _capture.Set(VideoCaptureProperties.FrameHeight, resolution.Height);
            }

            double actualWidth = _capture.Get(VideoCaptureProperties.FrameWidth);
            double actualHeight = _capture.Get(VideoCaptureProperties.FrameHeight);

            Debug.WriteLine($"Camera Resolution: {actualWidth}x{actualHeight}");
        }

        public async Task PreviewCameraAsync(string cameraName)
        {
            CameraStatusChanged?.Invoke(this, new CameraStatusEventArgs(CameraStatus.CameraConfiguring));
            await StopAsync();
            var config = _activeConfig ?? new CameraConfig();
            config.Name = cameraName;

            await OpenCameraAsync(config);
        }

        public async Task<CameraConfig> LoadAsync()
        {
            try
            {
                if (!File.Exists(_configPath))
                {
                    CameraStatusChanged?.Invoke(this, new CameraStatusEventArgs(CameraStatus.CameraNotConfigured));
                    await Task.Delay(1000);
                    return new CameraConfig();
                }

                var json = await File.ReadAllTextAsync(_configPath);
                return JsonConvert.DeserializeObject<CameraConfig>(json)
                       ?? new CameraConfig();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to load camera.config.json: {ex.Message}");
                CameraStatusChanged?.Invoke(this, new CameraStatusEventArgs(CameraStatus.CameraError));
                await Task.Delay(1000);
                return new CameraConfig();
            }
        }

        public async Task SaveAsync(CameraConfig config)
        {
            try
            {
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                await File.WriteAllTextAsync(_configPath, json);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to save config: {ex.Message}");
                CameraStatusChanged?.Invoke(this, new CameraStatusEventArgs(CameraStatus.CameraError));
            }
        }
    }
}

