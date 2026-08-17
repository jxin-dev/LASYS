using LASYS.Cleanup.UI.Presenters;
using LASYS.Cleanup.UI.Views.Configuration;
using LASYS.Shared.Cleanup.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LASYS.Cleanup.UI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMvp(this IServiceCollection services)
        {
            services.AddTransient<IConfigurationView, ConfigurationForm>();
            services.AddTransient<ConfigurationPresenter>();
            return services;
        }
        public static IServiceCollection AddCleanup(this IServiceCollection services)
        {
            services.AddSingleton<IScheduleSettingsService, ScheduleSettingsService>();
            services.AddSingleton<ICleanupRunnerService, CleanupRunnerService>();
            services.AddSingleton<ICleanupTaskSchedulerService, CleanupTaskSchedulerService>();
            return services;
        }
    }
}
