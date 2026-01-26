using LASYS.Domain.DeviceSettings;

namespace LASYS.Application.Interfaces
{
    public interface IDeviceConfigJsonRepository
    {
        Task<DeviceConfiguration> LoadAsync();
        Task SaveAsync(DeviceConfiguration config);
    }
}
