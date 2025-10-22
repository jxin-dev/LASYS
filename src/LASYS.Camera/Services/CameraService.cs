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

        public event Action? PreviewStarted;

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

        public async Task StartPreviewAsync(CameraDevice camera, PictureBox previewBox)
        {
            _firstFrameShown = false;
            StopPreview(previewBox);

            _capture = new VideoCapture(camera.Index);
            if (!_capture.IsOpened())
                throw new InvalidOperationException($"Cannot open camera: {camera.Name}");

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            await Task.Run(() =>
            {
                using var frame = new Mat();
                while (!token.IsCancellationRequested)
                {
                    if (!_capture.Read(frame) || frame.Empty())
                        continue;

                    using var bitmap = BitmapConverter.ToBitmap(frame);
                    if (!_firstFrameShown)
                    {
                        _firstFrameShown = true;
                        PreviewStarted?.Invoke(); // fire event once
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

                    Thread.Sleep(30); // ~30 FPS
                }
                previewBox.Invoke(() =>
                {
                    previewBox.Image?.Dispose();
                    previewBox.Image = null; // clear when stopped
                });
            }, token);
           
        }

        public void StopPreview(PictureBox previewBox)
        {
            if (_cts == null)
                return;

            _cts.Cancel();

            // Give background loop a short time to exit gracefully
            Task.Delay(100).Wait();

            try
            {
                _capture?.Release();
                _capture?.Dispose();
                _capture = null;
            }
            catch { /* ignore any release errors */ }

            _cts.Dispose();
            _cts = null;

            // ✅ Clear the PictureBox safely (on UI thread)
            if (previewBox.InvokeRequired)
            {
                previewBox.Invoke(() =>
                {
                    previewBox.Image?.Dispose();
                    previewBox.Image = null;  // makes it blank
                });
            }
            else
            {
                previewBox.Image?.Dispose();
                previewBox.Image = null; // makes it blank
            }
        }

        //public void StopPreview()
        //{
        //    _cts?.Cancel();
        //    _capture?.Release();
        //    _capture = null;
        //}

    }
}
