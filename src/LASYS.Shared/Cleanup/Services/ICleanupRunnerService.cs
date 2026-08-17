using LASYS.Shared.Cleanup.Models;

namespace LASYS.Shared.Cleanup.Services
{
    public interface ICleanupRunnerService
    {
        Task<int> RunAsync(CancellationToken cancellationToken = default);
    }
}
