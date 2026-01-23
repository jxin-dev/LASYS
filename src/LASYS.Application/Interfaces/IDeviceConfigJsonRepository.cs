using LASYS.Domain.DeviceSettings;

namespace LASYS.Application.Interfaces
{
    public interface IDeviceConfigJsonRepository
    {
        DeviceConfiguration Load();
        void Save(DeviceConfiguration config);
    }
}
