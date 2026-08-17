using System.Text.Json;
using LASYS.Shared.Cleanup.Models;

namespace LASYS.Shared.Cleanup.Services
{
    public sealed class ScheduleSettingsService : IScheduleSettingsService
    {
        private readonly string _settingsDirectory;
        private readonly string _settingsFile;
        public ScheduleSettingsService()
        {
            _settingsDirectory = AppContext.BaseDirectory;
            _settingsFile = Path.Combine(_settingsDirectory, "cleanupsettings.json");

        }

        public ScheduleSettings Current { get; private set; } = new();

        public ScheduleSettings Load()
        {
            var defaultCleanupFolder = Path.Combine(Directory.GetParent(Directory.GetParent(AppContext.BaseDirectory)!.FullName)!.FullName, "labelfiles");
            var defaultScheduleSettings = new ScheduleSettings();
            defaultScheduleSettings.CleanupFolder = defaultCleanupFolder;

            if (!File.Exists(_settingsFile))
            {
                if (!Directory.Exists(defaultScheduleSettings.CleanupFolder))
                    Directory.CreateDirectory(defaultScheduleSettings.CleanupFolder);
                
                Current = defaultScheduleSettings;
                return defaultScheduleSettings;
            }

            try
            {
                ScheduleSettings settings;

                string json = File.ReadAllText(_settingsFile);

                settings = JsonSerializer.Deserialize<ScheduleSettings>(json) ?? defaultScheduleSettings;

                if (string.IsNullOrWhiteSpace(settings.CleanupFolder))
                {
                    settings.CleanupFolder = defaultCleanupFolder;
                    if (!Directory.Exists(settings.CleanupFolder))
                        Directory.CreateDirectory(settings.CleanupFolder);
                }

                Current = settings;
                return settings;
            }
            catch (JsonException)
            {
                if (!Directory.Exists(defaultScheduleSettings.CleanupFolder))
                    Directory.CreateDirectory(defaultScheduleSettings.CleanupFolder);

                Current = defaultScheduleSettings;
                return defaultScheduleSettings;
            }
            catch (IOException)
            {
                if (!Directory.Exists(defaultScheduleSettings.CleanupFolder))
                    Directory.CreateDirectory(defaultScheduleSettings.CleanupFolder);
                
                Current = defaultScheduleSettings;
                return defaultScheduleSettings;
            }
        }

        public ScheduleSettings Load(string jsonPath)
        {
            var defaultScheduleSettings = new ScheduleSettings();
            var defaultCleanupFolder = defaultScheduleSettings.GetDefaultCleanupFolder();

            defaultScheduleSettings.CleanupFolder = defaultCleanupFolder;

            if (!File.Exists(jsonPath))
            {
                if (!Directory.Exists(defaultScheduleSettings.CleanupFolder))
                    Directory.CreateDirectory(defaultScheduleSettings.CleanupFolder);

                Current = defaultScheduleSettings;
                return defaultScheduleSettings;
            }

            try
            {
                ScheduleSettings settings;

                string json = File.ReadAllText(jsonPath);

                settings = JsonSerializer.Deserialize<ScheduleSettings>(json) ?? defaultScheduleSettings;

                if (string.IsNullOrWhiteSpace(settings.CleanupFolder))
                {
                    settings.CleanupFolder = defaultCleanupFolder;
                    if (!Directory.Exists(settings.CleanupFolder))
                        Directory.CreateDirectory(settings.CleanupFolder);
                }

                Current = settings;
                return settings;
            }
            catch (JsonException)
            {
                if (!Directory.Exists(defaultScheduleSettings.CleanupFolder))
                    Directory.CreateDirectory(defaultScheduleSettings.CleanupFolder);

                Current = defaultScheduleSettings;
                return defaultScheduleSettings;
            }
            catch (IOException)
            {
                if (!Directory.Exists(defaultScheduleSettings.CleanupFolder))
                    Directory.CreateDirectory(defaultScheduleSettings.CleanupFolder);

                Current = defaultScheduleSettings;
                return defaultScheduleSettings;
            }
        }

        public void Save(ScheduleSettings settings)
        {
            if (settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            Directory.CreateDirectory(_settingsDirectory);

            JsonSerializerOptions options = new()
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(settings, options);

            File.WriteAllText(_settingsFile, json);
            Current = settings;
        }
    }
}
