using System.Data;
using LASYS.Application.Interfaces.Persistence;
using LASYS.Application.Interfaces.Persistence.Repositories;
using LASYS.Application.Interfaces.Services;
using LASYS.Infrastructure.Hardware.Barcode;
using LASYS.Infrastructure.Hardware.Camera;
using LASYS.Infrastructure.Hardware.Printers.Sato;
using LASYS.Infrastructure.Logging;
using LASYS.Infrastructure.OCR;
using LASYS.Infrastructure.Persistence.Connection;
using LASYS.Infrastructure.Persistence.Repositories;
using LASYS.Infrastructure.Services.Session;
using Microsoft.Extensions.DependencyInjection;

namespace LASYS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddPersistence();
        services.AddBarcodeAnalyzerServices();
        services.AddCameraServices();
        services.AddOCRServices();
        services.AddSatoLabelPrinterServices();

        services.AddLoggingServices();

        services.AddSessionServices();

        return services;
    }

    public static IServiceCollection AddSessionServices(this IServiceCollection services)
    {
        services.AddSingleton<ISessionTracker, SessionTracker>();

        return services;
    }
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        //Connection
        services.AddSingleton<DatabaseSettings>();
        services.AddSingleton<IDbConnectionFactory, DapperContext>();
        services.AddScoped<IDbConnection>(sp =>
        {
            var context = sp.GetRequiredService<DapperContext>();
            return context.CreateConnectionAsync().GetAwaiter().GetResult();
        });

        //Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();

        return services;
    }

    public static IServiceCollection AddLoggingServices(this IServiceCollection services)
    {
        services.AddSingleton<ILogService, FileLogService>();

        return services;
    }

  
    private static IServiceCollection AddBarcodeAnalyzerServices(this IServiceCollection services)
    {
        services.AddSingleton<IBarcodeService, BarcodeService>();
        return services;
    }

    private static IServiceCollection AddCameraServices(this IServiceCollection services)
    {
        services.AddSingleton<ICameraService, CameraService>();
        return services;
    }

    private static IServiceCollection AddOCRServices(this IServiceCollection services)
    {
        services.AddScoped<ICalibrationService, CalibrationService>();
        services.AddSingleton<IOCRService, OCRService>();

        return services;
    }
    private static IServiceCollection AddSatoLabelPrinterServices(this IServiceCollection services)
    {
        services.AddSingleton<IPrinterService, PrinterService>();
        return services;
    }
}
