using LASYS.Application.Interfaces;
using LASYS.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace LASYS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddTransient<IUserRepository, UserRepository>();
        return services;
    }
}
