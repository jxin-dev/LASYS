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

        Task InitializeAsync();
        Task StopAsync();
        Task StartStreamingAsync(Action<Mat, Bitmap> onFrameCaptured, Func<DrawingSize> getTargetResolution);
        void ReleaseCamera();
        bool IsCameraReady();
        void SetFocus(int focusValue);
        bool IsStreaming { get; }

        //
        IReadOnlyList<string> GetCameras();
        int GetCameraIndex(string cameraName);

        // Configuration management
        event EventHandler<CameraConfigEventArgs> CameraConfigIssue;
        Task<CameraConfig> LoadCameraConfigAsync();
        Task SaveCameraConfigAsync(CameraConfig config);
        Dictionary<string, Resolution> GetCameraResolutions();
        event EventHandler<CameraNotificationEventArgs> CameraNotification;
        void RestartApplication();
        void SetResolution(string resolution);

        Task PreviewCameraAsync(string cameraName);


    }
}
