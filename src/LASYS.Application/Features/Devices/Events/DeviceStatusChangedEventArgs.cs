using LASYS.Application.Features.Devices.Models;

namespace LASYS.Application.Features.Devices.Events
{
    public sealed class DeviceStatusChangedEventArgs : EventArgs
    {
        public DeviceStatus Status { get; }
        public DeviceStatusChangedEventArgs(DeviceStatus status)
        {
            Status = status;
        }
    }
}
