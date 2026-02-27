using System.Diagnostics;
using LASYS.Application.Contracts;
using LASYS.Application.Events;
using LASYS.Application.Interfaces;
using Newtonsoft.Json;

namespace LASYS.Infrastructure.Camera
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

        public Dictionary<string, Resolution> GetCameraResolutions()
        {
            return new Dictionary<string, Resolution>
            {
                ["HD / 720p"] = new Resolution
                {
                    Width = 1280,
                    Height = 720,
                    AspectRatio = "16:9",
                    Notes = "Standard high definition"
                },
                ["Full HD / 1080p"] = new Resolution
                {
                    Width = 1920,
                    Height = 1080,
                    AspectRatio = "16:9",
                    Notes = "Most webcams, monitors, streaming"
                },
                ["4K UHD / 2160p"] = new Resolution
                {
                    Width = 3840,
                    Height = 2160,
                    AspectRatio = "16:9",
                    Notes = "Ultra HD"
                }
            };
        }

        public void RestartApplication()
        {
           string exePath = Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;
            if (string.IsNullOrEmpty(exePath))
            {
                Console.Error.WriteLine("Unable to determine executable path for restart.");
                return;
            }
            try
            {
                Process.Start(exePath);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to restart application: {ex.Message}");
                CameraConfigIssue?.Invoke(this, new CameraConfigEventArgs("Failed to restart application"));
            }
        }
    }
}
