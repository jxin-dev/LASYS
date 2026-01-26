using LASYS.Application.Interfaces;
using LASYS.Domain.DeviceSettings;
using Newtonsoft.Json;

namespace LASYS.Infrastructure.Repositories
{
    public class DeviceConfigJsonRepository : IDeviceConfigJsonRepository
    {
        private readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "device_config.json");
        public async Task<DeviceConfiguration> LoadAsync()
        {
            try
            {
                if (!File.Exists(_filePath))
                    return new DeviceConfiguration();

                var json = await File.ReadAllTextAsync(_filePath);
                return JsonConvert.DeserializeObject<DeviceConfiguration>(json)
                       ?? new DeviceConfiguration();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to load config: {ex.Message}");
                return new DeviceConfiguration();
            }
        }

        public async Task SaveAsync(DeviceConfiguration config)
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
