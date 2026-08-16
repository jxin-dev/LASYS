using LASYS.Cleanup.UI.Enums;

namespace LASYS.Cleanup.UI.Models
{
    public sealed class ScheduleSettings
    {
        public string CleanupFolder { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
        public int RetentionValue { get; set; } = 1;
        public RetentionUnit RetentionUnit { get; set; } = RetentionUnit.Months;

        public ScheduleFrequency Frequency { get; set; } = ScheduleFrequency.Daily;
        public TimeSpan RunTime { get; set; } = new(2, 0, 0);
    }
}
