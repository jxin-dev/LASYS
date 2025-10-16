using LASYS.DesktopApp.Presenters;
using LASYS.DesktopApp.Presenters.Interfaces;
using LASYS.DesktopApp.Views.Forms;
using LASYS.DesktopApp.Views.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LASYS.DesktopApp
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            var builder = Host.CreateApplicationBuilder();
            builder.Services.AddTransient<ISplashView, SplashForm>();
            builder.Services.AddTransient<ISplashPresenter, SplashPresenter>();

            var host = builder.Build();

            using (var scope = host.Services.CreateScope())
            {
                var provider = scope.ServiceProvider;
                var view = provider.GetRequiredService<ISplashView>();
                var presenter = provider.GetRequiredService<ISplashPresenter>();

                presenter.Initialize(view);
                (view as SplashForm)?.SetPresenter(presenter);

                Application.Run((Form)view);
            }
        }
    }
}