using System.Drawing;
using LASYS.Application.Contracts;
using LASYS.Application.Events;
using OpenCvSharp;
using DrawingSize = System.Drawing.Size;
namespace LASYS.Application.Interfaces
{
    public interface ICameraService : IDisposable
    {
        Mat? LastCapturedFrame { get; set; }
        Mat? GetSnapshot();

        event EventHandler<CameraStatusEventArgs> CameraStatusChanged;
        event EventHandler CameraDisconnected;
        event EventHandler CameraConnected;

        CameraInfo? ResolveCamera(CameraConfig config);
        Task InitializeAsync();
        Task StopAsync();
        Task StartStreamingAsync(Action<Mat, Bitmap> onFrameCaptured, Func<DrawingSize> getTargetResolution);
        void ReleaseCamera();
        bool IsCameraReady();

        bool IsStreaming { get; }

    }
}
