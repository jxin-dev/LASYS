using LASYS.Application.Interfaces;
using LASYS.Application.Services;
using LASYS.BarcodeAnalyzer.Interfaces;
using LASYS.Camera.Interfaces;
using LASYS.DesktopApp.Presenters;
using LASYS.DesktopApp.Views.Forms;
using LASYS.DesktopApp.Views.Interfaces;
using LASYS.DesktopApp.Views.UserControls;
using LASYS.SatoLabelPrinter.Interfaces;
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


            services.AddTransient<ErrorForm>(sp =>
            {
                var view = new ErrorForm();
                new ErrorPresenter(view);
                return view;
            });

            services.AddTransient<IErrorView>(sp => sp.GetRequiredService<ErrorForm>());


            services.AddTransient<IWorkOrdersView, WorkOrdersControl>();
            services.AddTransient<WorkOrdersPresenter>();

            services.AddTransient<ILabelPrintingView, LabelPrintingControl>();
            services.AddTransient<LabelPrintingPresenter>();

            services.AddTransient<IWebCameraView, WebCameraControl>();
            services.AddTransient<WebCameraPresenter>();

            services.AddTransient<IOCRCalibrationView, OCRCalibrationControl>();
            services.AddTransient<OCRCalibrationPresenter>();

            services.AddSingleton<IPrinterManagementView, PrinterManagementControl>();
            services.AddSingleton<PrinterManagementPresenter>();


            // Services

            services.AddScoped<IPrintingService, PrintingService>();


            return services;
        }

    }
}
