using LASYS.Application.Interfaces;
using LASYS.Domain.DeviceSettings;
using LASYS.Domain.OCR;
using Newtonsoft.Json;

namespace LASYS.Infrastructure.Repositories
{
    public class OCRConfigJsonRepository : IOCRConfigJsonRepository
    {
        private readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ocr_config.json");

        public async Task<OCRConfiguration> LoadAsync()
        {
            try
            {
                if (!File.Exists(_filePath))
                    return new OCRConfiguration();

                var json = await File.ReadAllTextAsync(_filePath);
                return JsonConvert.DeserializeObject<OCRConfiguration>(json)
                       ?? new OCRConfiguration();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to load config: {ex.Message}");
                return new OCRConfiguration();
            }
        }

        public async Task SaveAsync(OCRConfiguration config)
        {
            try
            {
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                await File.WriteAllTextAsync(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to save config: {ex.Message}");
                throw;
            }
        }
    }
}
