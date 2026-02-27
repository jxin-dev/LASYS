using LASYS.Application.Events;

namespace LASYS.Application.Interfaces
{
    public interface IDeviceManager
    {
        Task StartJobAsync(CancellationToken token);
        event EventHandler<DeviceStatusChangedEventArgs> StatusChanged;
    }
}
