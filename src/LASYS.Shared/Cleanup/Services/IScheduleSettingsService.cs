using LASYS.Shared.Cleanup.Models;

namespace LASYS.Shared.Cleanup.Services
{
    public interface IScheduleSettingsService
    {
        public ScheduleSettings Load();
        ScheduleSettings Current { get; }
        public void Save(ScheduleSettings settings);
    }
}
