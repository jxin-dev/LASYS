using LASYS.Application.Features.Devices.Enums;
using LASYS.Application.Features.Devices.Extensions;
using LASYS.Application.Features.Devices.Models;

namespace LASYS.Application.Features.Devices.Factories
{
    public sealed class DeviceStatusFactory
    {
        public static DeviceStatus Create(DeviceType device,
            DeviceStatusCode statusCode,
            string? descriptionOverride = null)
        {
            var info = statusCode.GetStatusInfo(device);

            return new DeviceStatus(
                device,
                statusCode,
                info.Message,
                descriptionOverride ?? info.Description);
        }
    }                                                  
}
