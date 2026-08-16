using System;
using LASYS.Cleanup.Features.Cleanup;
using LASYS.Cleanup.Models;
using Microsoft.Extensions.DependencyInjection;

namespace LASYS.Cleanup
{
    internal class Program
    {
        private static async Task<int> Main(string[] args)
        {
            try
            {
                string settingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"InnovaThinkCorporation","LASYS-Cleanup");

                string settingsPath =Path.Combine(settingsDirectory,"settings.json");

                if (!File.Exists(settingsPath))
                {
                    return 1;
                }

                ServiceCollection services = new();

                services.AddSingleton<
                    ICleanupSettingsReader,
                    CleanupSettingsReader>();

                services.AddSingleton<
                    ICleanupRunner,
                    CleanupRunner>();

                using ServiceProvider provider = services.BuildServiceProvider();

                ICleanupSettingsReader settingsReader = provider.GetRequiredService<ICleanupSettingsReader>();

                ICleanupRunner cleanupRunner = provider.GetRequiredService<ICleanupRunner>();

                ScheduleSettings settings = settingsReader.Load(settingsPath);

                return await cleanupRunner.RunAsync(settings);
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
