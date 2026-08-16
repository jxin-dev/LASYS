using LASYS.Cleanup.UI.Features.Schedule;
using LASYS.Cleanup.UI.Models;
using LASYS.Cleanup.UI.Views.Configuration;

namespace LASYS.Cleanup.UI.Presenters
{
    public class ConfigurationPresenter
    {
        private readonly IConfigurationView _view;
        private readonly ICleanupSettingsRepository _cleanupSettingsRepository;
        private readonly IScheduleCleanupTask _scheduleCleanupTask;
        public IConfigurationView View => _view;
        public ConfigurationPresenter(IConfigurationView view, ICleanupSettingsRepository cleanupSettingsRepository, IScheduleCleanupTask scheduleCleanupTask)
        {
            _view = view;
            _view.SaveRequested += OnSaveRequested;

            _cleanupSettingsRepository = cleanupSettingsRepository;
            _scheduleCleanupTask = scheduleCleanupTask;

            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                ScheduleSettings settings =
                    _cleanupSettingsRepository.Load();

                _view.LoadSettings(
                    settings.CleanupFolder,
                    settings.RetentionValue,
                    settings.RetentionUnit,
                    settings.Frequency,
                    settings.RunTime);
            }
            catch (Exception ex)
            {
                _view.ShowError(
                    $"Failed to load cleanup settings.\n\n{ex.Message}");
            }
        }
        private void OnSaveRequested(object? sender, EventArgs e)
        {
            try
            {
                ScheduleSettings settings = new()
                {
                    CleanupFolder = _view.CleanupFolder,
                    RetentionValue = _view.RetentionValue,
                    RetentionUnit = _view.RetentionUnit,
                    Frequency = _view.Frequency,
                    RunTime = _view.RunTime
                };

                _cleanupSettingsRepository.Save(settings);

                CleanupSchedule schedule = new()
                {
                    Frequency = settings.Frequency,
                    WeeklyDay = DayOfWeek.Monday,
                    MonthlyDay = 1,
                    Time = settings.RunTime
                };

                string cleanupExePath = Path.Combine(AppContext.BaseDirectory, "LASYS.Cleanup.exe");

                _scheduleCleanupTask.CreateOrUpdateTask(cleanupExePath, schedule);

                _view.ShowSuccess(
                    "Cleanup settings saved successfully.");
            }
            catch (Exception ex)
            {
                _view.ShowError(
                    $"Failed to save cleanup settings.\n\n{ex.Message}");
            }
        }

    }
}
