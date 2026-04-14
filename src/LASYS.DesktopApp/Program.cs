using System.Diagnostics;
using LASYS.Application;
using LASYS.Application.Features.Authentication.AutoLogin;
using LASYS.Application.Interfaces.Context;
using LASYS.Application.Interfaces.Services;
using LASYS.DesktopApp.Extensions;
using LASYS.DesktopApp.Presenters;
using LASYS.Infrastructure;
using MediatR;
using Microsoft.Extensions.Configuration;
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

#if DEBUG
            //Debugger.Launch(); //auto attach debugger
#endif

            VelopackApp.Build().Run();

            ApplicationConfiguration.Initialize();

            var host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(AppContext.BaseDirectory); 
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    var config = context.Configuration;

                    services.AddMvp(); // from our extension
                    services.AddApplication(); // from our application layer
                    services.AddInfrastructure(config); // from our infrastructure layer
                })
                .Build();

            //var configuration = host.Services.GetRequiredService<IConfiguration>();
            //var conn = configuration.GetConnectionString("TestConnection");

            var splashPresenter = host.Services.GetRequiredService<SplashPresenter>();
            splashPresenter.View.ShowDialog();

            //Read username from args
            var args = Environment.GetCommandLineArgs();
            var usernameArg = args.FirstOrDefault(a => a.StartsWith("--username="));
            var username = usernameArg?.Split("=")[1];

            bool isAuthenticated = false;

            if (!string.IsNullOrEmpty(username))
            {
                var mediator = host.Services.GetRequiredService<IMediator>();

                var currentUser = host.Services.GetRequiredService<ICurrentUser>();
                var sessionTracker = host.Services.GetRequiredService<ISessionTracker>();

                var result = mediator.Send(new AutoLoginCommand(username)).GetAwaiter().GetResult();

                if (result.IsSuccess && result.Value != null)
                {
                    isAuthenticated = true;

                    var user = result.Value;
                    currentUser.SetUser(
                        user.UserCode,
                        user.UserName,
                        user.SectionId,
                        user.RoleCode,
                        user.PlantCode,
                        user.FirstName,
                        user.LastName,
                        user.MiddleName,
                        user.Nickname,
                        user.Position,
                        user.DepartmentCode,
                        user.SectionName,
                        user.ImagePath);

                    sessionTracker.StartSession();

                }
            }

            if (!isAuthenticated)
            {
                var loginPresenter = host.Services.GetRequiredService<LoginPresenter>();
                if (loginPresenter.View.ShowDialog() != DialogResult.OK)
                {
                    System.Windows.Forms.Application.Exit(); // user cancelled login, exit the application
                    return; // user cancelled login
                }
            }

            var mainPresenter = host.Services.GetRequiredService<MainPresenter>();
            System.Windows.Forms.Application.Run(mainPresenter.View);

        }
    }
}