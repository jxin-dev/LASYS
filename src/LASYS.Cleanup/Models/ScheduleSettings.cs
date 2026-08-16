
using LASYS.Cleanup.Enums;

namespace LASYS.Cleanup.Models
{
    public sealed class ScheduleSettings
    {
        public string CleanupFolder { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public int RetentionValue { get; set; } = 1;
        public RetentionUnit RetentionUnit { get; set; } = RetentionUnit.Months;

    }
}
