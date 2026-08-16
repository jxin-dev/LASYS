using System.Text.Json;
using LASYS.Cleanup.UI.Models;

namespace LASYS.Cleanup.UI.Features.Schedule
{
    public sealed class CleanupSettingsRepository : ICleanupSettingsRepository
    {
        private readonly string _settingsDirectory;
        private readonly string _settingsFile;
        public CleanupSettingsRepository()
        {
            //_settingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "InnovaThinkCorporation", "LASYS-Cleanup");
            _settingsDirectory = AppContext.BaseDirectory;
            _settingsFile = Path.Combine(_settingsDirectory, "settings.json");
        }
        public ScheduleSettings Load()
        {
            if (!File.Exists(_settingsFile))
            {
                return new ScheduleSettings();
            }

            try
            {
                ScheduleSettings settings;

                string json = File.ReadAllText(_settingsFile);

                settings = JsonSerializer.Deserialize<ScheduleSettings>(json) ?? new ScheduleSettings();

                if (string.IsNullOrWhiteSpace(settings.CleanupFolder))
                {
                    settings.CleanupFolder = settings.GetDefaultCleanupFolder();
                }

                return settings;
            }
            catch (JsonException)
            {
                // Invalid/corrupted settings file.
                // Return default settings.
                return new ScheduleSettings();
            }
            catch (IOException)
            {
                // File could not be read.
                return new ScheduleSettings();
            }
        }

        public void Save(ScheduleSettings settings)
        {
            if (settings is null)
            {
                throw new ArgumentNullException(
                    nameof(settings));
            }

            Directory.CreateDirectory(
                _settingsDirectory);

            JsonSerializerOptions options = new()
            {
                WriteIndented = true
            };

            string json =
                JsonSerializer.Serialize(
                    settings,
                    options);

            File.WriteAllText(
                _settingsFile,
                json);
        }
    }
}
