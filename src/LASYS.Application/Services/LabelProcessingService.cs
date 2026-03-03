using LASYS.Application.Events;
using LASYS.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace LASYS.Application.Services
{
    public class LabelProcessingService : ILabelProcessingService
    {
        public event EventHandler<DeviceStatusChangedEventArgs>? StatusChanged;

        private readonly ILogger<LabelProcessingService> _logger;
   

        public Task StartJobAsync(CancellationToken token)
        {
            StatusChanged?.Invoke(this, new DeviceStatusChangedEventArgs("System Ready"));



            return Task.CompletedTask;
        }
    }
}
