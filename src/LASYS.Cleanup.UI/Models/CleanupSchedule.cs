using LASYS.Cleanup.UI.Enums;

namespace LASYS.Cleanup.UI.Models
{
    public class CleanupSchedule
    {
        public ScheduleFrequency Frequency { get; init; }
        public DayOfWeek? WeeklyDay { get; init; } = DayOfWeek.Monday;
        public int? MonthlyDay { get; init; } = 1;
        public TimeSpan Time { get; init; }
    }
}
