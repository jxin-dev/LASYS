using LASYS.Shared.Cleanup.Enums;

namespace LASYS.Shared.Cleanup.Models
{
    public class CleanupTaskInfo
    {
        public bool Exists { get; init; }
        public bool Enabled { get; init; }
        public ScheduleFrequency? Frequency { get; init; }
        public DayOfWeek? WeeklyDay { get; init; }
        public int? MonthlyDay { get; init; }
        public DateTime? NextRun { get; init; }
        public DateTime? LastRun { get; init; }
        public int LastResult { get; init; }
        public string? ExecutablePath { get; init; }
        public string? Arguments { get; init; }
        public TimeSpan? ScheduledTime { get; init; }
    }
}
