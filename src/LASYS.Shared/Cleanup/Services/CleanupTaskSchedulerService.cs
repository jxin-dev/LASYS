using LASYS.Shared.Cleanup.Enums;
using LASYS.Shared.Cleanup.Models;
using Microsoft.Win32.TaskScheduler;

namespace LASYS.Shared.Cleanup.Services
{
    public sealed class CleanupTaskSchedulerService : ICleanupTaskSchedulerService
    {
        private const string TaskName = "LASYS Print Job Cleanup";
        public void CreateOrUpdateTask(string cleanupExePath, CleanupSchedule schedule)
        {
            if (string.IsNullOrWhiteSpace(cleanupExePath))
                throw new ArgumentException(
                    "Cleanup executable path is required.",
                    nameof(cleanupExePath));

            if (!File.Exists(cleanupExePath))
                throw new FileNotFoundException(
                    "Cleanup executable was not found.",
                    cleanupExePath);

            using TaskService taskService = new();

            TaskDefinition taskDefinition = taskService.NewTask();

            taskDefinition.RegistrationInfo.Description =
                "Automatically cleans old LASYS Print Job files.";

            // Task settings
            taskDefinition.Settings.Enabled = true;
            taskDefinition.Settings.Hidden = true;
            taskDefinition.Settings.AllowDemandStart = true;
            taskDefinition.Settings.StartWhenAvailable = true;

            // No execution time limit
            taskDefinition.Settings.ExecutionTimeLimit = TimeSpan.Zero;

            // Run under currently logged-in user
            taskDefinition.Principal.LogonType = TaskLogonType.InteractiveToken;
            // Run with elevated privileges
            //taskDefinition.Principal.RunLevel = TaskRunLevel.Highest;

            // Add Daily / Weekly / Monthly trigger
            AddTrigger(
                taskDefinition,
                schedule);

            // Run LASYS.Cleanup.exe
            taskDefinition.Actions.Add(
                new ExecAction(
                    cleanupExePath,
                    string.Empty,
                    Path.GetDirectoryName(cleanupExePath)));

            // Creates the task if it doesn't exist.
            // Updates it if it already exists.
            taskService.RootFolder.RegisterTaskDefinition(
                TaskName,
                taskDefinition,
                TaskCreation.CreateOrUpdate,
                null,
                null,
                TaskLogonType.InteractiveToken);
        }
        private static void AddTrigger(TaskDefinition taskDefinition, CleanupSchedule schedule)
        {
            DateTime startTime =
                DateTime.Today.Add(schedule.Time);

            switch (schedule.Frequency)
            {
                case ScheduleFrequency.Daily:

                    taskDefinition.Triggers.Add(
                        new DailyTrigger
                        {
                            StartBoundary = startTime,
                            DaysInterval = 1
                        });

                    break;

                case ScheduleFrequency.Weekly:

                    if (schedule.WeeklyDay is null)
                    {
                        throw new ArgumentException(
                            "Weekly day is required.");
                    }

                    taskDefinition.Triggers.Add(
                        new WeeklyTrigger
                        {
                            StartBoundary = startTime,
                            DaysOfWeek = ConvertDayOfWeek(
                                schedule.WeeklyDay.Value),
                            WeeksInterval = 1
                        });

                    break;

                case ScheduleFrequency.Monthly:

                    if (schedule.MonthlyDay is null)
                    {
                        throw new ArgumentException(
                            "Monthly day is required.");
                    }

                    if (schedule.MonthlyDay < 1 ||
                        schedule.MonthlyDay > 28)
                    {
                        throw new ArgumentOutOfRangeException(
                            nameof(schedule.MonthlyDay),
                            "Monthly day must be between 1 and 28.");
                    }

                    taskDefinition.Triggers.Add(
                        new MonthlyTrigger
                        {
                            StartBoundary = startTime,

                            DaysOfMonth =
                            [
                                schedule.MonthlyDay.Value
                            ],

                            MonthsOfYear =
                                MonthsOfTheYear.AllMonths
                        });

                    break;

                default:

                    throw new ArgumentOutOfRangeException(
                        nameof(schedule.Frequency));
            }
        }

        private static DaysOfTheWeek ConvertDayOfWeek(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Sunday =>
                    DaysOfTheWeek.Sunday,

                DayOfWeek.Monday =>
                    DaysOfTheWeek.Monday,

                DayOfWeek.Tuesday =>
                    DaysOfTheWeek.Tuesday,

                DayOfWeek.Wednesday =>
                    DaysOfTheWeek.Wednesday,

                DayOfWeek.Thursday =>
                    DaysOfTheWeek.Thursday,

                DayOfWeek.Friday =>
                    DaysOfTheWeek.Friday,

                DayOfWeek.Saturday =>
                    DaysOfTheWeek.Saturday,

                _ => throw new ArgumentOutOfRangeException(
                    nameof(day))
            };
        }
        private static DayOfWeek? ConvertDayOfWeek(DaysOfTheWeek days)
        {
            if (days.HasFlag(DaysOfTheWeek.Sunday))
                return DayOfWeek.Sunday;

            if (days.HasFlag(DaysOfTheWeek.Monday))
                return DayOfWeek.Monday;

            if (days.HasFlag(DaysOfTheWeek.Tuesday))
                return DayOfWeek.Tuesday;

            if (days.HasFlag(DaysOfTheWeek.Wednesday))
                return DayOfWeek.Wednesday;

            if (days.HasFlag(DaysOfTheWeek.Thursday))
                return DayOfWeek.Thursday;

            if (days.HasFlag(DaysOfTheWeek.Friday))
                return DayOfWeek.Friday;

            if (days.HasFlag(DaysOfTheWeek.Saturday))
                return DayOfWeek.Saturday;

            return null;
        }
        public void DeleteTask()
        {
            using TaskService taskService = new();

            if (taskService.GetTask(TaskName) is not null)
            {
                taskService.RootFolder.DeleteTask(
                    TaskName,
                    false);
            }
        }

        public CleanupTaskInfo? GetTaskInfo()
        {
            using TaskService taskService = new();

            Microsoft.Win32.TaskScheduler.Task? task =
                taskService.GetTask(TaskName);

            if (task is null)
            {
                return null;
            }

            Trigger? trigger =
                task.Definition.Triggers.FirstOrDefault();

            ExecAction? action =
                task.Definition.Actions
                    .OfType<ExecAction>()
                    .FirstOrDefault();

            ScheduleFrequency? frequency = null;
            DayOfWeek? weeklyDay = null;
            int? monthlyDay = null;
            TimeSpan? scheduledTime = null;

            switch (trigger)
            {
                case DailyTrigger daily:

                    frequency =
                        ScheduleFrequency.Daily;

                    scheduledTime =
                        daily.StartBoundary.TimeOfDay;

                    break;

                case WeeklyTrigger weekly:

                    frequency =
                        ScheduleFrequency.Weekly;

                    scheduledTime =
                        weekly.StartBoundary.TimeOfDay;

                    weeklyDay =
                        ConvertDayOfWeek(
                            weekly.DaysOfWeek);

                    break;

                case MonthlyTrigger monthly:

                    frequency =
                        ScheduleFrequency.Monthly;

                    scheduledTime =
                        monthly.StartBoundary.TimeOfDay;

                    monthlyDay =
                        monthly.DaysOfMonth
                            .FirstOrDefault();

                    break;
            }

            return new CleanupTaskInfo
            {
                Exists = true,

                Enabled = task.Enabled,

                Frequency = frequency,

                WeeklyDay = weeklyDay,

                MonthlyDay = monthlyDay,

                NextRun = task.NextRunTime,

                LastRun = task.LastRunTime,

                LastResult = task.LastTaskResult,

                ExecutablePath = action?.Path,

                Arguments = action?.Arguments,

                ScheduledTime = scheduledTime
            };
        }

        public bool TaskExists()
        {
            using TaskService taskService = new();

            return taskService.GetTask(TaskName) is not null;
        }
    }
}
