using LASYS.Application.Contracts;

namespace LASYS.Application.Interfaces
{
    public interface ICameraEnumerator
    {
        IReadOnlyList<CameraInfo> GetCameras();
    }
}
