using LASYS.DesktopApp.Core.Interfaces;
using LASYS.DesktopApp.Core.Services;
using LASYS.DesktopApp.Presenters;
using LASYS.DesktopApp.Views.Forms;
using LASYS.DesktopApp.Views.Interfaces;
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
            // Services


            return services;
        }
    }
}
