using System.Diagnostics;
using System.Reflection;
using LASYS.Cleanup.UI.Extensions;
using LASYS.Cleanup.UI.Presenters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LASYS.Cleanup.UI
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif
            ApplicationConfiguration.Initialize();

            var host = Host.CreateDefaultBuilder()
               .ConfigureAppConfiguration((context, config) =>
               {
                   //config.SetBasePath(AppContext.BaseDirectory);
                   //config.AddJsonFile("settings.json", optional: false, reloadOnChange: true);
               })
               .ConfigureServices((context, services) =>
               {
                   var config = context.Configuration;
                   services.AddMvp(); // from our extension
                   services.AddCleanup();
               })
               .Build();

            var configurationPresenter = host.Services.GetRequiredService<ConfigurationPresenter>();
            System.Windows.Forms.Application.Run(configurationPresenter.View.Form);
            
        }
    }
}