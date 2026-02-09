using LASYS.Camera.Events;
using LASYS.Camera.Interfaces;
using LASYS.Camera.Models;
using Newtonsoft.Json;

namespace LASYS.Camera.Services
{
    public class CameraConfigStore : ICameraConfig
    {
        private readonly string _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "camera.config.json");

        public event EventHandler<CameraConfigEventArgs>? CameraConfigIssue;

        public async Task<CameraConfig> LoadAsync()
        {
            try
            {
                if (!File.Exists(_configPath))
                {
                    CameraConfigIssue?.Invoke(this, new CameraConfigEventArgs("Camera configuration file not found."));
                    await Task.Delay(1000);
                    return new CameraConfig();
                }

                var json = await File.ReadAllTextAsync(_configPath);
                return JsonConvert.DeserializeObject<CameraConfig>(json)
                       ?? new CameraConfig();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to load camera.config.json: {ex.Message}");
                CameraConfigIssue?.Invoke(this, new CameraConfigEventArgs("Failed to load camera.config.json"));
                await Task.Delay(1000);
                return new CameraConfig();
            }
        }

        public async Task SaveAsync(CameraConfig config)
        {
            try
            {
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                await File.WriteAllTextAsync(_configPath, json);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to save config: {ex.Message}");
                CameraConfigIssue?.Invoke(this, new CameraConfigEventArgs("Failed to save camera.config.json"));
                throw;
            }
        }
    }
}
