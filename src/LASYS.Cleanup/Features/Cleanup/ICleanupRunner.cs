using LASYS.Cleanup.Models;

namespace LASYS.Cleanup.Features.Cleanup
{
    public interface ICleanupRunner
    {
        Task<int> RunAsync(ScheduleSettings settings, CancellationToken cancellationToken = default);
    }
}
