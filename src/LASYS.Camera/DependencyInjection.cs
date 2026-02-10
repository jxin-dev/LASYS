using LASYS.Camera.Interfaces;
using LASYS.Camera.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LASYS.Camera
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCameraServices(this IServiceCollection services)
        {

            services.AddScoped<ICameraConfig, CameraConfigStore>();
            services.AddScoped<ICameraEnumerator, CameraEnumerator>();
            // Camera implementations
            services.AddTransient<IPreviewCameraService, PreviewCameraService>();
            services.AddSingleton<ICameraService, CameraService>();

            return services;
        }
    }
}
