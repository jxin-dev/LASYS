using LASYS.Application;
using LASYS.DesktopApp.Extensions;
using LASYS.DesktopApp.Presenters;
using LASYS.Infrastructure;
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
               services.AddApplication(); // from our application layer
               services.AddInfrastructure(); // from our infrastructure layer
           })
           .Build();

            var splashPresenter = host.Services.GetRequiredService<SplashPresenter>();
            splashPresenter.View.ShowDialog();

            var loginPresenter = host.Services.GetRequiredService<LoginPresenter>();
            if (loginPresenter.View.ShowDialog() != DialogResult.OK)
                return; // user cancelled login

            var mainPresenter = host.Services.GetRequiredService<MainPresenter>();
            System.Windows.Forms.Application.Run(mainPresenter.View);

        }
    }
}