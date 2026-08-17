using LASYS.Shared.Cleanup.Enums;

namespace LASYS.Shared.Cleanup.Models
{
    public class CleanupSchedule
    {
        public ScheduleFrequency Frequency { get; init; }
        public DayOfWeek? WeeklyDay { get; init; } = DayOfWeek.Monday;
        public int? MonthlyDay { get; init; } = 1;
        public TimeSpan Time { get; init; }
    }
}
