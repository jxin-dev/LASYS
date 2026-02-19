using LASYS.SatoLabelPrinter.Interfaces;
using LASYS.SatoLabelPrinter.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LASYS.SatoLabelPrinter
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSatoLabelPrinterServices(this IServiceCollection services)
        {
            services.AddSingleton<IPrinterService, PrinterService>();
            return services;
        }
    }
}
