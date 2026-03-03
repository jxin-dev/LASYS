using LASYS.Application.Events;

namespace LASYS.Application.Interfaces
{
    public interface ILabelProcessingService
    {
        Task StartJobAsync(CancellationToken token);
        event EventHandler<DeviceStatusChangedEventArgs> StatusChanged;
    }
}
