using LASYS.Application.Interfaces;
using LASYS.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LASYS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddLabelServices();
        return services;
    }
    private static IServiceCollection AddLabelServices(this IServiceCollection services)
    {
        services.AddTransient<ILabelProcessingService, LabelProcessingService>();
        return services;
    }

}
