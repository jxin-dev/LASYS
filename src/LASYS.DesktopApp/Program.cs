using LASYS.DesktopApp.Core.Interfaces;
using LASYS.DesktopApp.Extensions;
using LASYS.DesktopApp.Presenters;
using LASYS.DesktopApp.Views.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Velopack;

namespace LASYS.DesktopApp
{
    internal static class Program
    {
       
        [STAThread]
        static void Main()
        {

            VelopackApp.Build().Run();

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