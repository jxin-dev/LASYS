using LASYS.Shared.Cleanup.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LASYS.Cleanup
{
    internal class Program
    {
        private static async Task<int> Main(string[] args)
        {
            try
            {
                ServiceCollection services = new();

                services.AddSingleton<ICleanupRunnerService, CleanupRunnerService>();
                services.AddSingleton<IScheduleSettingsService, ScheduleSettingsService>();


                using ServiceProvider provider = services.BuildServiceProvider();

                ICleanupRunnerService cleanupRunner = provider.GetRequiredService<ICleanupRunnerService>();

                return await cleanupRunner.RunAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(
                    $"Cleanup failed: {ex.Message}");

                return 1;
            }
        }
    }
}
