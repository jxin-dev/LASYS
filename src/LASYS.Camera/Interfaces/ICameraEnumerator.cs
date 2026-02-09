using LASYS.Camera.Models;

namespace LASYS.Camera.Interfaces
{
    public interface ICameraEnumerator
    {
        IReadOnlyList<CameraInfo> GetCameras();
    }
}
