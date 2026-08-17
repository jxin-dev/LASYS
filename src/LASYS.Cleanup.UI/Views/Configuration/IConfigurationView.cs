using LASYS.Shared.Cleanup.Enums;

namespace LASYS.Cleanup.UI.Views.Configuration
{
    public interface IConfigurationView
    {
        string CleanupFolder { get; }
        int RetentionValue { get; }
        RetentionUnit RetentionUnit { get; }
        ScheduleFrequency Frequency { get; }
        TimeSpan RunTime { get; }
        void LoadSettings(string cleanupFolder, int retentionValue, RetentionUnit retentionUnit, ScheduleFrequency frequency, TimeSpan runTime);

        void ShowError(string message);
        void ShowSuccess(string message);
        Form Form { get; }
        event EventHandler? SaveRequested;
        void Show();
        void Close();

    }
}
