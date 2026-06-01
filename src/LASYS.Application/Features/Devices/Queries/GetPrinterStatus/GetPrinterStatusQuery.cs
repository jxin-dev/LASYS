using LASYS.Application.Features.Devices.Models;
using MediatR;

namespace LASYS.Application.Features.Devices.Queries.GetPrinterStatus
{
    public sealed record GetPrinterStatusQuery : IRequest<DeviceStatus>;
}
