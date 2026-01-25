using DirectShowLib;
using LASYS.Camera.Interfaces;
using LASYS.Camera.Models;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace LASYS.Camera.Services
{
    public class CameraService : ICameraService
    {
        private VideoCapture? _capture;
        private CancellationTokenSource? _cts;
        private bool _firstFrameShown = false;

        //public event Action? PreviewStarted;

        private TaskCompletionSource<bool>? _previewStartedTcs;
        public Task PreviewStartedAsync =>_previewStartedTcs?.Task ?? Task.CompletedTask;
        public List<CameraDevice> GetAvailableCameras()
        {
            var devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            var list = new List<CameraDevice>();

            for (int i = 0; i < devices.Length; i++)
            {
                list.Add(new CameraDevice
                {
                    Index = i,
                    Name = devices[i].Name
                });
            }
            return list;
        }

        public VideoCapture GetCamera(int index)
        {
            var capture = new VideoCapture(index);
            if (!capture.IsOpened())
                throw new InvalidOperationException($"Camera {index} could not be opened.");

            return capture;
        }
        private void PreviewLoop(PictureBox previewBox, CancellationToken token)
        {
            int failedReads = 0;

            try
            {
                using var frame = new Mat();

                while (!token.IsCancellationRequested)
                {
                    if (!_capture!.Read(frame) || frame.Empty())
                    {
                        failedReads++;
                        if (failedReads > 10) // ~300ms = disconnect
                            break;

                        Thread.Sleep(30);
                        continue;
                    }

                    failedReads = 0;

                    using var bitmap = BitmapConverter.ToBitmap(frame);

                    if (!_firstFrameShown)
                    {
                        _firstFrameShown = true;
                        _previewStartedTcs?.TrySetResult(true);
                    }

                    if (previewBox.InvokeRequired)
                    {
                        previewBox.Invoke(() =>
                        {
                            previewBox.Image?.Dispose();
                            previewBox.Image = (Bitmap)bitmap.Clone();
                        });
                    }
                    else
                    {
                        previewBox.Image?.Dispose();
                        previewBox.Image = (Bitmap)bitmap.Clone();
                    }

                    Thread.Sleep(30);
                }
            }
            catch (Exception ex)
            {
                _previewStartedTcs?.TrySetException(ex);
            }
            finally
            {
                CleanupPreview(previewBox);
            }
        }
        private void CleanupPreview(PictureBox previewBox)
        {
            try
            {
                _capture?.Release();
                _capture?.Dispose();
            }
            catch { }

            _capture = null;

            _cts?.Dispose();
            _cts = null;

            if (previewBox.IsHandleCreated)
            {
                previewBox.Invoke(() =>
                {
                    previewBox.Image?.Dispose();
                    previewBox.Image = null;
                });
            }
        }

        public Task StartPreviewAsync(CameraDevice camera, PictureBox previewBox)
        {
            if (_cts != null)
                throw new InvalidOperationException("Preview already running.");

            _previewStartedTcs = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            _firstFrameShown = false;

            _capture = new VideoCapture(camera.Index);
            if (!_capture.IsOpened())
                throw new InvalidOperationException($"Cannot open camera: {camera.Name}");

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            // fire-and-forget background loop
            _ = Task.Run(() => PreviewLoop(previewBox, token), token);

            return Task.CompletedTask;
            
        }

        public void StopPreview(PictureBox previewBox)
        {
            if (_cts == null)
                return;

            _cts.Cancel();
        }

    }
}
