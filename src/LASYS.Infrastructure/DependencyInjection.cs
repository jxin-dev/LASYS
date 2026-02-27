using LASYS.Application.Interfaces;
using LASYS.Infrastructure.Barcode;
using LASYS.Infrastructure.Camera;
using LASYS.Infrastructure.OCR;
using LASYS.Infrastructure.Repositories;
using LASYS.Infrastructure.SatoLabelPrinter;
using Microsoft.Extensions.DependencyInjection;

namespace LASYS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
    {
        services.AddTransient<IUserRepository, UserRepository>();
        return services;
    }

    public static IServiceCollection AddBarcodeAnalyzerServices(this IServiceCollection services)
    {
        services.AddSingleton<IBarcodeService, BarcodeService>();
        return services;
    }

    public static IServiceCollection AddCameraServices(this IServiceCollection services)
    {

        services.AddScoped<ICameraConfig, CameraConfigStore>();
        services.AddScoped<ICameraEnumerator, CameraEnumerator>();
        // Camera implementations
        services.AddTransient<IPreviewCameraService, PreviewCameraService>();
        services.AddSingleton<ICameraService, CameraService>();

        return services;
    }

    public static IServiceCollection AddOCRServices(this IServiceCollection services)
    {
        services.AddScoped<ICalibrationService, CalibrationService>();
        services.AddSingleton<IOCRService, OCRService>();

        return services;
    }
    public static IServiceCollection AddSatoLabelPrinterServices(this IServiceCollection services)
    {
        services.AddSingleton<IPrinterService, PrinterService>();
        return services;
    }
}
