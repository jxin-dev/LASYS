using LASYS.Application.Features.LabelProcessing.Abstractions;
using LASYS.Application.Interfaces;
using LASYS.DesktopApp.Presenters;
using LASYS.DesktopApp.Views.Forms;
using LASYS.DesktopApp.Views.Interfaces;
using LASYS.DesktopApp.Views.UserControls;
using Microsoft.Extensions.DependencyInjection;

namespace LASYS.DesktopApp.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMvp(this IServiceCollection services)
        {

            services.AddTransient<ISplashView, SplashForm>();
            services.AddTransient<SplashPresenter>();

            services.AddTransient<ILoginView, LoginForm>();
            services.AddTransient<LoginPresenter>();

            services.AddSingleton<IMainView, MainForm>();
            //services.AddTransient<IMainView>(sp => sp.GetRequiredService<MainForm>());
            services.AddSingleton<MainPresenter>();

            //services.AddSingleton<IMainView, MainForm>();
            //services.AddSingleton<MainPresenter>(sp =>
            //{
            //    var view = sp.GetRequiredService<IMainView>();
            //    return new MainPresenter(view, sp);
            //});

            services.AddTransient<IErrorView, ErrorForm>();
            services.AddTransient<ErrorPresenter>();


            services.AddTransient<IWorkOrdersView, WorkOrdersControl>();
            services.AddTransient<WorkOrdersPresenter>();

            services.AddTransient<ILabelPrintingView, LabelPrintingControl>();
            services.AddTransient<LabelPrintingPresenter>();

            services.AddSingleton<IVisionSettingsView, VisionSettingsControl>();
            services.AddSingleton<VisionSettingsPresenter>();

            services.AddSingleton<IPrinterManagementView, PrinterManagementControl>();
            services.AddSingleton<PrinterManagementPresenter>();

            services.AddSingleton<IBarcodeScannerView, BarcodeScannerControl>();
            services.AddSingleton<BarcodeScannerPresenter>();

            // Services



            return services;
        }

    }
}
