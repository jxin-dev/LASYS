using LASYS.Cleanup.UI.Models;

namespace LASYS.Cleanup.UI.Features.Schedule
{
    public interface IScheduleCleanupTask
    {
        void CreateOrUpdateTask(string cleanupExePath, CleanupSchedule schedule);
        void DeleteTask();
        bool TaskExists();
        CleanupTaskInfo? GetTaskInfo();
    }
}
