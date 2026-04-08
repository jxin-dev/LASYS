using LASYS.Application.Interfaces.Services;
using Microsoft.Extensions.Options;

namespace LASYS.Infrastructure.Services.Media
{
    public class ImageService : IImageService
    {
        private readonly ImageSettings _settings;
        private readonly HttpClient _httpClient;

        public ImageService(IOptions<ImageSettings> settings, HttpClient httpClient)
        {
            _settings = settings.Value;
            _httpClient = httpClient;
        }

        public Task<string> GetExternalImageUrlAsync(string userCode)
        {
            var url = string.Format(_settings.ExternalTemplate, userCode);
            return Task.FromResult(url);
        }


        public async Task<string> GetUserImageUrlAsync(string userCode)
        {
            var fileName = $"{userCode}.jpg";
            var localPath = Path.Combine(_settings.BasePath, fileName);
            // 1. Check local cache
            if (!File.Exists(localPath))
            {
                try
                {
                    var externalUrl = await GetExternalImageUrlAsync(userCode);
                    var bytes = await _httpClient.GetByteArrayAsync(externalUrl);

                    Directory.CreateDirectory(_settings.BasePath);
                    await File.WriteAllBytesAsync(localPath, bytes);
                }
                catch
                {
                    // fallback to default image
                    return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _settings.DefaultImage);
                }
            }
            return localPath;
        }
    }
}
