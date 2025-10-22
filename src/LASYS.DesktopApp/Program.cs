using LASYS.DesktopApp.Core.Interfaces;
using LASYS.DesktopApp.Extensions;
using LASYS.DesktopApp.Presenters;
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
            var host = Host.CreateDefaultBuilder()
           .ConfigureServices(services =>
           {
               services.AddMvp(); // from our extension
               services.AddDevices(); // from our extension
           })
           .Build();

            var factory = host.Services.GetRequiredService<IViewFactory>();
            var splashView = factory.Create<ISplashView, SplashPresenter>();
            splashView.ShowView();

        }
    }
}