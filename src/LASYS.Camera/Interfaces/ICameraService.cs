using LASYS.Camera.Events;
using LASYS.Camera.Models;
using OpenCvSharp;
using DrawingSize = System.Drawing.Size;
namespace LASYS.Camera.Interfaces
{
    public interface ICameraService : IDisposable
    {
        Mat LastCapturedFrame { get; set; }
        event EventHandler<CameraStatusEventArgs> CameraStatusChanged;
        event EventHandler CameraDisconnected;
        event EventHandler CameraConnected;

        CameraInfo? ResolveCamera(CameraConfig config);
        Task InitializeAsync();
        Task StopAsync();
        Task StartStreamingAsync(Action<Mat, Bitmap> onFrameCaptured, Func<DrawingSize> getTargetResolution);
        void ReleaseCamera();
        bool IsCameraReady();

    }
}
