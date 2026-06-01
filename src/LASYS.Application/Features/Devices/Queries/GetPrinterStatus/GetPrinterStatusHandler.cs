using LASYS.Application.Features.Devices.Models;
using LASYS.Application.Interfaces.Services;
using MediatR;

namespace LASYS.Application.Features.Devices.Queries.GetPrinterStatus
{
    public sealed class GetPrinterStatusHandler : IRequestHandler<GetPrinterStatusQuery, DeviceStatus>
    {
        private readonly IPrinterService _printerService;

        public GetPrinterStatusHandler(IPrinterService printerService)
        {
            _printerService = printerService;
        }

        public Task<DeviceStatus> Handle(GetPrinterStatusQuery request, CancellationToken cancellationToken)
        {
            var status = _printerService.CurrentStatus;
            return Task.FromResult(status);
        }
    }
}
