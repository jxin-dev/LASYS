using OpenCvSharp;

namespace LASYS.Application.Interfaces
{
    public interface IPreviewCameraService : IDisposable
    {
        void StartCamera(int cameraIndex = 0);
        void StopCamera();

        event Action<Mat> FrameReady;
    }
}
