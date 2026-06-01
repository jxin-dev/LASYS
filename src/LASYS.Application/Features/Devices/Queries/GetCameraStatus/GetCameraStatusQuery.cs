using LASYS.Application.Features.Devices.Models;
using MediatR;

namespace LASYS.Application.Features.Devices.Queries.GetCameraStatus
{
    public sealed record GetCameraStatusQuery : IRequest<DeviceStatus>;
}
