using LASYS.Shared.Cleanup.Enums;

namespace LASYS.Shared.Cleanup.Models
{
    public sealed class ScheduleSettings
    {
        public string CleanupFolder { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
        public int RetentionValue { get; set; } = 30;
        public RetentionUnit RetentionUnit { get; set; } = RetentionUnit.Days;

        public ScheduleFrequency Frequency { get; set; } = ScheduleFrequency.Daily;
        public TimeSpan RunTime { get; set; } = new(12, 0, 0);

        public string GetDefaultCleanupFolder()
        {
            return Path.Combine(AppContext.BaseDirectory, "labelfiles");
        }
    }
}
