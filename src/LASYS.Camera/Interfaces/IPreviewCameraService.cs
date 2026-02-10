using OpenCvSharp;

namespace LASYS.Camera.Interfaces
{
    public interface IPreviewCameraService : IDisposable
    {
        void StartCamera(int cameraIndex = 0);
        void StopCamera();

        event Action<Mat> FrameReady;
    }
}
