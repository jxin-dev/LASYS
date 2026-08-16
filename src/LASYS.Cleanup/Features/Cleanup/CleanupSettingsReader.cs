using System.Text.Json;
using System.Text.Json.Serialization;
using LASYS.Cleanup.Models;

namespace LASYS.Cleanup.Features.Cleanup
{
    public sealed class CleanupSettingsReader : ICleanupSettingsReader
    {
        public ScheduleSettings Load(string settingsPath)
        {
            if (string.IsNullOrWhiteSpace(settingsPath))
            {
                throw new ArgumentException("Settings file path is required.", nameof(settingsPath));
            }

            if (!File.Exists(settingsPath))
            {
                throw new FileNotFoundException("Cleanup settings file was not found.", settingsPath);
            }

            string json = File.ReadAllText(settingsPath);

            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true,
                Converters =
                {
                    new JsonStringEnumConverter()
                }
            };

            return JsonSerializer.Deserialize<ScheduleSettings>(json, options)
                   ?? throw new InvalidOperationException("Cleanup settings file is empty or invalid.");
        }
    }
}
