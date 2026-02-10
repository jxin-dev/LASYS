using LASYS.Application.Interfaces;
using LASYS.Application.Services;
using LASYS.Camera.Interfaces;
using LASYS.Camera.Services;
using LASYS.DesktopApp.Presenters;
using LASYS.DesktopApp.Views.Forms;
using LASYS.DesktopApp.Views.Interfaces;
using LASYS.DesktopApp.Views.UserControls;
using LASYS.OCR.Interfaces;
using LASYS.OCR.Services;
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
                new SplashPresenter(view, cameraConfig, cameraService);
                return view;
            });
            services.AddTransient<ISplashView>(sp => sp.GetRequiredService<SplashForm>());

            services.AddTransient<LoginForm>(sp =>
            {
                var view = new LoginForm();
                new LoginPresenter(view);
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

            services.AddTransient<IPrinterManagementView, PrinterManagementControl>();
            services.AddTransient<PrinterManagementPresenter>();


            // Services

            services.AddScoped<IPrintingService, PrintingService>();


            return services;
        }

    }
}
