using LASYS.BarcodeAnalyzer.Interfaces;
using LASYS.BarcodeAnalyzer.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LASYS.BarcodeAnalyzer
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBarcodeAnalyzerServices(this IServiceCollection services)
        {
            services.AddSingleton<IBarcodeService, BarcodeService>();
            return services;
        }
    }
}
