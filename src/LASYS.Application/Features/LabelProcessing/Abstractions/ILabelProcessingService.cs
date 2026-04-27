using System.Drawing;
using LASYS.Application.Common.Enums;
using LASYS.Application.Events;
using LASYS.Application.Features.LabelProcessing.Contracts;

namespace LASYS.Application.Features.LabelProcessing.Abstractions
{
    public interface ILabelProcessingService
    {
        Task InitializeDevicesAsync();
        void LoadLabelTemplateAsync(string templatePath);
        Task StartJobAsync(Size viewerSize, StartLabelJobRequest request);
        void SetUserDecision(OperatorDecision action);
        void Pause();
        void Resume();
        void Stop();

        event EventHandler<LogEventArgs> LogGenerated;
        event EventHandler<PrintJobState> PrintControlsStateChanged;
        event EventHandler<OperatorDecisionRequiredEventArgs> DecisionRequired;
        event EventHandler<DeviceStatusEventArgs> DeviceStatusChanged;
        void NotifyStatus(PrintJobState status);

        IEnumerable<DeviceStatusEventArgs> GetCurrentDeviceStatuses();
    }
}
