using LASYS.Application.Features.BatchPrinting.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LASYS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddBatchPrinting();
        return services;
    }
    public static IServiceCollection AddBatchPrinting(this IServiceCollection services)
    {
        services.AddSingleton<IPrintJobController, PrintJobController>();
        services.AddSingleton<IBatchPrintProcessService, BatchPrintProcessService>();
        return services;
    }


}
