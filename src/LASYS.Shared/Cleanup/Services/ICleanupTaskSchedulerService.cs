using LASYS.Shared.Cleanup.Models;

namespace LASYS.Shared.Cleanup.Services
{
    public interface ICleanupTaskSchedulerService
    {
        void CreateOrUpdateTask(string cleanupExePath, CleanupSchedule schedule);
        void DeleteTask();
        bool TaskExists();
        CleanupTaskInfo? GetTaskInfo();
    }
}
