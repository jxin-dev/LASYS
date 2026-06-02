using LASYS.Application.Features.Devices.Enums;

namespace LASYS.Application.Features.Devices.Models
{
    public sealed record DeviceStatus(
        DeviceType Device,
        DeviceStatusCode Status,
        string Message,
        string Description)
    {
        public bool IsConnected => Status == DeviceStatusCode.Connected;

        public bool IsFaulted =>
            Status is DeviceStatusCode.Error
                  or DeviceStatusCode.Timeout
                  or DeviceStatusCode.NotDetected
                  or DeviceStatusCode.Disconnected;
    }
}
