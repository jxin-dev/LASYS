using LASYS.Cleanup.UI.Features.Schedule;
using LASYS.Cleanup.UI.Presenters;
using LASYS.Cleanup.UI.Views.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LASYS.Cleanup.UI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMvp(this IServiceCollection services)
        {
            services.AddTransient<IConfigurationView, ConfigurationForm>();
            services.AddTransient<ConfigurationPresenter>();

            services.AddTransient<ICleanupSettingsRepository, CleanupSettingsRepository>();
            services.AddTransient<IScheduleCleanupTask, CleanupTaskScheduler>();

            return services;
        }
    }
}
