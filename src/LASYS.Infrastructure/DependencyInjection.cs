using System.Data;
using LASYS.Application.Interfaces.Context;
using LASYS.Application.Interfaces.Persistence;
using LASYS.Application.Interfaces.Persistence.Repositories;
using LASYS.Application.Interfaces.Persistence.TableMappings;
using LASYS.Application.Interfaces.Services;
using LASYS.Infrastructure.Hardware.Barcode;
using LASYS.Infrastructure.Hardware.Camera;
using LASYS.Infrastructure.Hardware.Printers.Sato;
using LASYS.Infrastructure.Logging;
using LASYS.Infrastructure.OCR;
using LASYS.Infrastructure.Persistence.Connection;
using LASYS.Infrastructure.Persistence.Repositories;
using LASYS.Infrastructure.Persistence.TableMappings;
using LASYS.Infrastructure.Services.Media;
using LASYS.Infrastructure.Services.Security;
using LASYS.Infrastructure.Services.Session;
using LASYS.Infrastructure.Services.WorkOrder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LASYS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddPersistence(config);
        services.AddBarcodeAnalyzerServices();
        services.AddCameraServices();
        services.AddOCRServices();
        services.AddSatoLabelPrinterServices();

        services.AddLoggingServices();

        services.AddSessionServices();

        services.AddImageServices(config);

        services.AddPermissionServices();

        return services;
    }


    public static IServiceCollection AddPermissionServices(this IServiceCollection services)
    {
        services.AddSingleton<IPermissionService, PermissionService>();
        return services;
    }
    public static IServiceCollection AddImageServices(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<ImageSettings>(config.GetSection("ImageSettings"));

        services.AddSingleton<HttpClient>();
        services.AddScoped<IImageService, ImageService>();

        return services;
    }

    public static IServiceCollection AddSessionServices(this IServiceCollection services)
    {
        services.AddSingleton<ISessionTracker, SessionTracker>();

        return services;
    }
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<DatabaseSettings>(config.GetSection("DatabaseSettings"));

        services.AddSingleton<IDatabaseEnvironment, DatabaseEnvironment>();

        services.AddScoped<IDbConnectionFactory, DapperContext>();

        //Connection
        //services.AddSingleton<DatabaseSettings>();
        //services.Configure<DatabaseSettings>(config.GetSection("DatabaseSettings"));
        //services.AddSingleton(sp => sp.GetRequiredService<IOptions<DatabaseSettings>>().Value);

        //services.AddSingleton<IDbConnectionFactory, DapperContext>();
        //services.AddScoped<IDbConnection>(sp =>
        //{
        //    var context = sp.GetRequiredService<DapperContext>();
        //    return context.CreateConnectionAsync().GetAwaiter().GetResult();
        //});

        //Table & Column Mapping
        services.AddScoped<IPrintTableResolver, PrintTableResolver>();
        services.AddScoped<INiceLabelColumnResolver, NiceLabelColumnResolver>();
        services.AddScoped<IProductColumnResolver, ProductColumnResolver>();
        services.AddScoped<IMasterLabelColumnResolver, MasterLabelColumnResolver>();
        services.AddScoped<ILabelInstructionColumnResolver,  LabelInstructionColumnResolver>();

        //Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IHrUserRepository, HrUserRepository>();
        services.AddScoped<ILabelInstructionRepository, LabelInstructionRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IMasterLabelRepository, MasterLabelRepository>();
        services.AddScoped<IPrintLabelRepository, PrintLabelRepository>();




        services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
        services.AddScoped<WorkOrderService>();

        services.AddScoped<IProductRepository, ProductRepository>();

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
        services.AddSingleton<INiceLabelTemplateService, NiceLabelTemplateService>();  
        return services;
    }
}
