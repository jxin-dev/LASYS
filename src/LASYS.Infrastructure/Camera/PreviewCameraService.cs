using LASYS.Application.Interfaces;
using OpenCvSharp;

namespace LASYS.Infrastructure.Camera
{
    public class PreviewCameraService : IPreviewCameraService, IDisposable
    {
        private VideoCapture? _capture;
        private bool _disposed = false;
        private CancellationTokenSource? _cts;

        // Event triggered when a new frame is ready
        public event Action<Mat>? FrameReady;

        public void StartCamera(int cameraIndex = 0)
        {
            StopCamera(); // Ensure previous capture is stopped

            _capture = new VideoCapture(cameraIndex, VideoCaptureAPIs.DSHOW);
            if (!_capture.IsOpened())
            {
                _capture.Dispose();
                _capture = null;
                throw new InvalidOperationException($"Unable to open camera at index {cameraIndex}.");
            }

            _cts = new CancellationTokenSource();
            StartCaptureLoop(_cts.Token);
        }

        private void StartCaptureLoop(CancellationToken token)
        {
            // Guard: ensure _capture is valid before starting
            if (_capture == null || !_capture.IsOpened())
                throw new InvalidOperationException("Cannot start capture loop: camera is not initialized.");

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    // Additional guard to stop if _capture becomes null/disposed
                    if (_capture == null || !_capture.IsOpened())
                        break;

                    try
                    {
                        using var frame = new Mat(); // Dispose frame automatically
                        
                        if (_capture.Read(frame) && !frame.Empty())
                            FrameReady?.Invoke(frame.Clone()); // Clone to avoid disposed Mat issues
                    }
                    catch (Exception ex)
                    {
                        // Optional: log or notify frame capture errors
                        Console.WriteLine($"Frame capture error: {ex.Message}");
                        break; // stop the loop if an exception occurs
                    }

                    // Use async delay to respect cancellation
                    try
                    {
                        await Task.Delay(30, token);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    //Thread.Sleep(30); // ~33 FPS
                }
            }, token);
        }


        public void StopCamera()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }

            if (_capture != null)
            {
                _capture.Release();
                _capture.Dispose();
                _capture = null;
            }
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                    StopCamera();

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~PreviewCameraService()
        {
            Dispose(false);
        }

        #endregion
    }
}
