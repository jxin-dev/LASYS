using LASYS.Application.Interfaces;
using LASYS.Application.Services;
using LASYS.Camera.Interfaces;
using LASYS.Camera.Services;
using LASYS.DesktopApp.Core.Interfaces;
using LASYS.DesktopApp.Core.Services;
using LASYS.DesktopApp.Presenters;
using LASYS.DesktopApp.Views.Forms;
using LASYS.DesktopApp.Views.Interfaces;
using LASYS.DesktopApp.Views.UserControls;
using LASYS.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace LASYS.DesktopApp.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMvp(this IServiceCollection services)
        {
            services.AddSingleton<IViewFactory, ViewFactory>();
            // MVP
            services.AddTransient<ISplashView, SplashForm>();
            services.AddTransient<SplashPresenter>();

            services.AddTransient<ILoginView, LoginForm>();
            services.AddTransient<LoginPresenter>();

            services.AddTransient<IMainView, MainForm>();
            services.AddTransient<MainPresenter>();


            services.AddTransient<WebCameraControl>();
            services.AddTransient<WorkOrdersControl>();

            // Services


            return services;
        }

        public static IServiceCollection AddDevices(this IServiceCollection services)
        {
            services.AddScoped<ICameraService, CameraService>();

            services.AddSingleton<IDeviceConfigJsonRepository, DeviceConfigJsonRepository>();
            services.AddSingleton<DeviceConfigService>();


            return services;
        }
    }
}
