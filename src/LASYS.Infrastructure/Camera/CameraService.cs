using System.Diagnostics;
using System.Drawing;
using LASYS.Application.Contracts;
using LASYS.Application.Events;
using LASYS.Application.Interfaces;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using DrawingSize = System.Drawing.Size;

namespace LASYS.Infrastructure.Camera
{
    public class CameraService : ICameraService
    {
        public event EventHandler<CameraStatusEventArgs>? CameraStatusChanged;
        public event EventHandler? CameraDisconnected;
        public event EventHandler? CameraConnected;

        private readonly ICameraConfig _cameraConfig;
        private readonly ICameraEnumerator _cameraEnumerator;

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

        public CameraService(ICameraConfig cameraConfig, ICameraEnumerator cameraEnumerator)
        {

            _cameraConfig = cameraConfig;
            _cameraEnumerator = cameraEnumerator;

            _cameraConfig.CameraConfigIssue += (s, e) =>
            {
                CameraStatusChanged?.Invoke(this,
                    new CameraStatusEventArgs(e.Message, true));
            };

            _cts = new CancellationTokenSource();
        }


        public async Task InitializeAsync()
        {
            CameraStatusChanged?.Invoke(this,
              new CameraStatusEventArgs("Initializing camera, please wait..."));

            await StopAsync();
            _activeConfig = await _cameraConfig.LoadAsync();
            await OpenCameraAsync(_activeConfig);
        }

        // ----------------------------------------------------
        // Camera resolution
        // ----------------------------------------------------
        public CameraInfo? ResolveCamera(CameraConfig config)
        {
            var cameras = _cameraEnumerator.GetCameras();

            return cameras.FirstOrDefault(c =>
                       c.Index == config.Index &&
                       string.Equals(c.Name, config.Name, StringComparison.OrdinalIgnoreCase))
                   ?? cameras.FirstOrDefault(c =>
                       string.Equals(c.Name, config.Name, StringComparison.OrdinalIgnoreCase));
        }


        private async Task OpenCameraAsync(CameraConfig config)
        {
            ReleaseCamera();

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));

            try
            {
                await Task.Run(() =>
                {
                    var cameraInfo = ResolveCamera(config);
                    if (cameraInfo == null)
                    {
                        CameraStatusChanged?.Invoke(this, new CameraStatusEventArgs(
                                $"Device unavailable ({config.Name})", true));
                        CameraDisconnected?.Invoke(this, EventArgs.Empty);
                        return;
                    }
                    var cameraIndex = cameraInfo.Index;

                    var capture = new VideoCapture(cameraIndex);
                    //var capture = new VideoCapture(cameraIndex, VideoCaptureAPIs.DSHOW);

                    if (!capture.IsOpened())
                    {
                        return; // hot-plug safe
                    }
                    capture.Set(VideoCaptureProperties.FrameWidth, config.FrameWidth);
                    capture.Set(VideoCaptureProperties.FrameHeight, config.FrameHeight);
                    capture.Set(VideoCaptureProperties.Fps, config.FrameRate);
                    _capture = capture;
                    //capture.Set(VideoCaptureProperties.FrameWidth, 640);
                    //capture.Set(VideoCaptureProperties.FrameHeight, 480);
                    //capture.Set(VideoCaptureProperties.Fps, 10);

                }, timeoutCts.Token);


                _hasReportedEmptyFrame = false;
                _isCameraConnected = false;
            }
            catch (OperationCanceledException)
            {
                if (!_cts.IsCancellationRequested)
                {
                    CameraStatusChanged?.Invoke(this,
                        new CameraStatusEventArgs(
                            "Camera initialization timed out.", true));
                    CameraDisconnected?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                CameraStatusChanged?.Invoke(this,
                    new CameraStatusEventArgs(ex.Message, true));
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
                    CameraStatusChanged?.Invoke(this, new CameraStatusEventArgs("Camera not initialized", true));
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

                        CameraStatusChanged?.Invoke(this,
                          new CameraStatusEventArgs("Unable to access the camera.", true));

                        CameraDisconnected?.Invoke(this, EventArgs.Empty);

                        Thread.Sleep(1000);
                        continue;
                    }

                    using var frame = new Mat();
                    using var resized = new Mat();

                    if (!TryReadFrame(frame) || frame.Empty())
                    {
                        HandleEmptyFrame(onFrameCaptured, getTargetResolution);

                        CameraStatusChanged?.Invoke(this,
                          new CameraStatusEventArgs("Unable to access the camera.", true));

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
                            resized.CopyTo(LastCapturedFrame);
                        }
                    }

                    ReportConnectedOnce();
                }

            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);


            return _streamingTask;
        }
        private void HandleFrameSafe(Mat resized, Action<Mat, Bitmap> onFrameCaptured)
        {
            var bitmap = BitmapConverter.ToBitmap(resized);
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
                        CameraStatusChanged?.Invoke(this,
                            new CameraStatusEventArgs("Unable to access the camera.", true));

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

            CameraStatusChanged?.Invoke(this,
                new CameraStatusEventArgs("Connected"));

            _isCameraConnected = true;
        }

        private void CreateAndCaptureEmptyFrame(DrawingSize size, Action<Mat, Bitmap> onFrameCaptured)
        {
            using var mat = new Mat(size.Height, size.Width, MatType.CV_8UC3, Scalar.All(0));
            using var bitmap = BitmapConverter.ToBitmap(mat);

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
            //_cts.Cancel();

            //if (_streamingTask != null)
            //    await _streamingTask;

            //ReleaseCamera();

            //_cts.Dispose();
            //_cts = new CancellationTokenSource();

            //_streamingTask = null;
            //_hasReportedEmptyFrame = false;
            //_isCameraConnected = false;
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

                return LastCapturedFrame.Clone();  // safe snapshot
            }
        }
    }
}

