using LASYS.Application.Contracts;
using LASYS.Application.Events;

namespace LASYS.Application.Interfaces
{
    public interface ICameraConfig
    {
        event EventHandler<CameraConfigEventArgs> CameraConfigIssue;
        Task<CameraConfig> LoadAsync();
        Task SaveAsync(CameraConfig config);
        Dictionary<string, Resolution> GetCameraResolutions();

        void RestartApplication();
    }
}
