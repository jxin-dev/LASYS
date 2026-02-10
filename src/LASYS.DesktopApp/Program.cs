using LASYS.Camera;
using LASYS.Camera.Services;
using LASYS.DesktopApp.Core.Interfaces;
using LASYS.DesktopApp.Extensions;
using LASYS.DesktopApp.Presenters;
using LASYS.DesktopApp.Presenters.Interfaces;
using LASYS.DesktopApp.Views.Forms;
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

               services.AddCameraServices(); // from camera library extension
           })
           .Build();

            var splashForm = host.Services.GetRequiredService<SplashForm>();
            splashForm.ShowDialog();

            var loginForm = host.Services.GetRequiredService<LoginForm>();
            if (loginForm.ShowDialog() != DialogResult.OK)
                return; // user cancelled login

            var mainPresenter = host.Services.GetRequiredService<MainPresenter>();
            var mainForm = (Form)mainPresenter.View;

            mainForm.Show();
            System.Windows.Forms.Application.Run(mainForm);


        }
    }
}