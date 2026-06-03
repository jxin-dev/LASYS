using LASYS.Application.Features.Devices.Models;
using LASYS.Application.Interfaces.Services.Camera;
using MediatR;

namespace LASYS.Application.Features.Devices.Queries.GetCameraStatus
{
    public sealed class GetCameraStatusHandler : IRequestHandler<GetCameraStatusQuery, DeviceStatus>
    {
        private readonly ICameraService _cameraService;
        public GetCameraStatusHandler(ICameraService cameraService)
        {
            _cameraService = cameraService;
        }
        public Task<DeviceStatus> Handle(GetCameraStatusQuery request, CancellationToken cancellationToken)
        {
            var status = _cameraService.CurrentStatus;
            return Task.FromResult(status);
        }
    }
}
