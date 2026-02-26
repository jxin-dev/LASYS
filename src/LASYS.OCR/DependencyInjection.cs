using LASYS.OCR.Interfaces;
using LASYS.OCR.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LASYS.OCR
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddOCRServices(this IServiceCollection services)
        {
            services.AddScoped<ICalibrationService, CalibrationService>();
            services.AddSingleton<IOCRService, OCRService>();

            return services;
        }
    }
}
