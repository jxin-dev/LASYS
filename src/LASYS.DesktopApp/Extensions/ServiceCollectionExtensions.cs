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

            services.AddTransient<SplashForm>(sp =>
            {
                var view = new SplashForm();
                var cameraConfig = sp.GetRequiredService<ICameraConfig>();
                var cameraService = sp.GetRequiredService<ICameraService>();
                var printerService = sp.GetRequiredService<IPrinterService>();
                var barcodeService = sp.GetRequiredService<IBarcodeService>();

                new SplashPresenter(view, cameraConfig, cameraService, printerService, barcodeService);
                return view;
            });
            services.AddTransient<ISplashView>(sp => sp.GetRequiredService<SplashForm>());

            services.AddTransient<LoginForm>(sp =>
            {
                var userRepo = sp.GetRequiredService<IUserRepository>();
                var view = new LoginForm();
                new LoginPresenter(view, userRepo);
                return view;
            });
            services.AddTransient<ILoginView>(sp => sp.GetRequiredService<LoginForm>());

            services.AddSingleton<IMainView, MainForm>();
            services.AddSingleton<MainPresenter>(sp =>
            {
                var view = sp.GetRequiredService<IMainView>();
                return new MainPresenter(view, sp);
            });


            //services.AddTransient<ErrorForm>(sp =>
            //{
            //    var labelProcessingService = sp.GetRequiredService<ILabelProcessingService>();
            //    var view = new ErrorForm();
            //    new ErrorPresenter(view, labelProcessingService);
            //    return view;
            //});

            //services.AddTransient<IErrorView>(sp => sp.GetRequiredService<ErrorForm>());

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
