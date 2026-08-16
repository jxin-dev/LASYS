using LASYS.Cleanup.Models;

namespace LASYS.Cleanup.Features.Cleanup
{
    public interface ICleanupSettingsReader
    {
        ScheduleSettings Load(string settingsPath);
    }
}
