using LASYS.Application.Features.Devices.Enums;

namespace LASYS.Application.Features.Devices.Models
{
    public sealed record DeviceStatus(
        DeviceType Device,
        DeviceStatusCode Status,
        bool IsConnected,
        string Message,
        string Description);
}
