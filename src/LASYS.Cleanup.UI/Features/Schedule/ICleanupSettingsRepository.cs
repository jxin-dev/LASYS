using LASYS.Cleanup.UI.Models;

namespace LASYS.Cleanup.UI.Features.Schedule
{
    public interface ICleanupSettingsRepository
    {
        ScheduleSettings Load();
        void Save(ScheduleSettings settings);
    }
}
