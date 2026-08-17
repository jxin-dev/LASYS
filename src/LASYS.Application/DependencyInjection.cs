using LASYS.Application.Features.BarcodeValidation;
using LASYS.Application.Features.BatchPrinting.Services;
using LASYS.Shared.Cleanup.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LASYS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddBatchPrinting();
        services.AddSingleton<Gs1BarcodeParser>(); // Required by barcode validation handlers to parse GS1 barcode data.
        services.AddCleanup();
        return services;
    }
    public static IServiceCollection AddBatchPrinting(this IServiceCollection services)
    {
        services.AddSingleton<IPrintJobController, PrintJobController>();
        services.AddSingleton<IBatchPrintProcessService, BatchPrintProcessService>();
        return services;
    }

    public static IServiceCollection AddCleanup(this IServiceCollection services)
    {
        services.AddSingleton<IScheduleSettingsService, ScheduleSettingsService>();
        return services;
    }

}
