using LASYS.Camera.Models;
using OpenCvSharp;

namespace LASYS.Camera.Interfaces
{
    public interface ICameraService
    {
        List<CameraDevice> GetAvailableCameras();
        VideoCapture GetCamera(int index);
        Task StartPreviewAsync(CameraDevice camera, PictureBox previewBox);
        void StopPreview(PictureBox previewBox);
        event Action? PreviewStarted;
    }
}
