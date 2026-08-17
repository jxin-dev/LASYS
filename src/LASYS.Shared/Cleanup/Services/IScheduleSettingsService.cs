using LASYS.Shared.Cleanup.Models;

namespace LASYS.Shared.Cleanup.Services
{
    public interface IScheduleSettingsService
    {
        public ScheduleSettings Load();
        public ScheduleSettings Load(string jsonPath);
        ScheduleSettings Current { get; }
        public void Save(ScheduleSettings settings);
    }
}
