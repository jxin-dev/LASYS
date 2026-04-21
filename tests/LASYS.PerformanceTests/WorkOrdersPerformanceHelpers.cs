using LASYS.Application.Common.Messaging;
using LASYS.Application;
using LASYS.Application.Features.LabelProcessing.GetWorkOrders;
using LASYS.Application.Interfaces.Services;
using LASYS.DesktopApp.Presenters;
using LASYS.DesktopApp.Views.Interfaces;
using LASYS.DesktopApp.Views.UserControls;
using LASYS.Infrastructure;
using LASYS.Infrastructure.Services.WorkOrder;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Xunit;

namespace LASYS.PerformanceTests;

internal static class WorkOrdersPerformanceHelpers
{
    public static double MeasureNextPageNavigationAverageMilliseconds(int rowCount, int warmupIterations = 1, int iterations = 3)
    {
        return ExecuteOnStaThread(() =>
        {
            using var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            using var view = new WorkOrdersControl();
            var presenter = new WorkOrdersPresenter(
                view,
                new NullMainView(),
                scope.ServiceProvider,
                scope.ServiceProvider.GetRequiredService<IMediator>(),
                scope.ServiceProvider.GetRequiredService<WorkOrderService>(),
                scope.ServiceProvider.GetRequiredService<ILogService>());

            WaitForPresenterIdle(presenter);

            var handlePageNoChangedAsync = typeof(WorkOrdersPresenter).GetMethod(
                "HandlePageNoChangedAsync",
                BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException("HandlePageNoChangedAsync not found.");

            for (int i = 0; i < warmupIterations; i++)
            {
                var warmupTask = (Task)handlePageNoChangedAsync.Invoke(presenter, new object[] { 2 })!;
                warmupTask.GetAwaiter().GetResult();
            }

            long totalTicks = 0;
            for (int i = 0; i < iterations; i++)
            {
                var start = Stopwatch.GetTimestamp();
                var task = (Task)handlePageNoChangedAsync.Invoke(presenter, new object[] { 2 })!;
                task.GetAwaiter().GetResult();
                totalTicks += Stopwatch.GetTimestamp() - start;
            }

            return totalTicks * 1000d / Stopwatch.Frequency / iterations;
        });
    }

    private static ServiceProvider BuildServiceProvider()
    {
        var appSettingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(appSettingsPath, optional: false, reloadOnChange: false)
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(config);
        services.AddApplication();
        services.AddInfrastructure(config);
        services.AddSingleton<ILogService, NullLogService>();

        return services.BuildServiceProvider();
    }

    private static string AppendConnectTimeout(string connectionString, int timeoutSeconds)
    {
        if (connectionString.Contains("Connect Timeout=", StringComparison.OrdinalIgnoreCase) ||
            connectionString.Contains("Connection Timeout=", StringComparison.OrdinalIgnoreCase))
        {
            return connectionString;
        }

        return connectionString.TrimEnd(';') + $";Connection Timeout={timeoutSeconds};";
    }

    private static void WaitForPresenterIdle(object presenter)
    {
        var isLoadingField = presenter.GetType().GetField("_isLoading", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("_isLoading not found.");

        var timeout = TimeSpan.FromSeconds(120);
        var start = Stopwatch.GetTimestamp();

        while ((bool)isLoadingField.GetValue(presenter)!)
        {
            if (Stopwatch.GetElapsedTime(start) > timeout)
                throw new TimeoutException("Timed out waiting for the initial work-order load to complete.");

            Thread.Sleep(50);
        }
    }

    private static T ExecuteOnStaThread<T>(Func<T> action)
    {
        T? result = default;
        Exception? threadError = null;

        var thread = new Thread(() =>
        {
            try
            {
                result = action();
            }
            catch (Exception ex)
            {
                threadError = ex;
            }
        });

        thread.IsBackground = true;
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (threadError != null)
            throw threadError;

        return result!;
    }

    private sealed class NullMainView : IMainView
    {
        public event EventHandler? WorkOrderRequested;
        public event EventHandler? VisionSettingsRequested;
        public event EventHandler? PrinterManagementRequested;
        public event EventHandler? BarcodeDeviceSetupRequested;
        public event EventHandler? FormClosingRequested;
        public event EventHandler? LogoutRequested;

        public void LoadView(UserControl control, bool cache = true) { }
        public void ShowUserInfo(string fullName, string sectionName, string? imagePath) { }
        public void CloseView() { }
    }

    private sealed class NullLogService : ILogService
    {
        public void Log(string message, MessageType type) { }
    }
}
