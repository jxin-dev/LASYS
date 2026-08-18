using LASYS.Cleanup.UI.Views.Configuration;
using LASYS.Shared.Cleanup.Models;
using LASYS.Shared.Cleanup.Services;

namespace LASYS.Cleanup.UI.Presenters
{
    public class ConfigurationPresenter
    {
        private readonly IConfigurationView _view;
        private readonly IScheduleSettingsService _scheduleSettingsService;
        private readonly ICleanupTaskSchedulerService _cleanupTaskSchedulerService;
        public IConfigurationView View => _view;
        public ConfigurationPresenter(IConfigurationView view, IScheduleSettingsService scheduleSettingsService, ICleanupTaskSchedulerService cleanupTaskSchedulerService)
        {
            _view = view;
            _view.SaveRequested += OnSaveRequested;

            _scheduleSettingsService = scheduleSettingsService;
            _cleanupTaskSchedulerService = cleanupTaskSchedulerService;

            LoadSettings();
            LoadTaskStatus();
        }

        private void LoadTaskStatus()
        {
            try
            {
                CleanupTaskInfo? taskInfo =
                    _cleanupTaskSchedulerService.GetTaskInfo();

                _view.LoadTaskInfo(taskInfo);
            }
            catch (Exception ex)
            {
                _view.ShowError(
                    $"Failed to load cleanup task status.\n\n{ex.Message}");
            }
        }

        private void LoadSettings()
        {
            try
            {
                ScheduleSettings settings = _scheduleSettingsService.Load();

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

                _scheduleSettingsService.Save(settings);

                CleanupSchedule schedule = new()
                {
                    Frequency = settings.Frequency,
                    WeeklyDay = DayOfWeek.Monday,
                    MonthlyDay = 1,
                    Time = settings.RunTime
                };

                string cleanupExePath = Path.Combine(AppContext.BaseDirectory, "LASYS.Cleanup.exe");

                _cleanupTaskSchedulerService.CreateOrUpdateTask(cleanupExePath, schedule);
                LoadTaskStatus();
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
