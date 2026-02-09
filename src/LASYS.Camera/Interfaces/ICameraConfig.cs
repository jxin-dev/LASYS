using LASYS.Camera.Events;
using LASYS.Camera.Models;

namespace LASYS.Camera.Interfaces
{
    public interface ICameraConfig
    {
        event EventHandler<CameraConfigEventArgs> CameraConfigIssue;
        Task<CameraConfig> LoadAsync();
        Task SaveAsync(CameraConfig config);
    }
}
