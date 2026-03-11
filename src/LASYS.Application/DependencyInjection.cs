using LASYS.Application.Features.LabelProcessing.Abstractions;
using LASYS.Application.Features.LabelProcessing.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LASYS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddLabelServices();
        return services;
    }
    private static IServiceCollection AddLabelServices(this IServiceCollection services)
    {
        services.AddSingleton<ILabelProcessingService, LabelProcessingService>();
        return services;
    }

}
