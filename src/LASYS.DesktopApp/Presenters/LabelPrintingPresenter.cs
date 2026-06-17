using System.Diagnostics;
using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Messaging;
using LASYS.Application.Events;
using LASYS.Application.Features.BatchPrinting.Commands.InitializeBatchPrint;
using LASYS.Application.Features.BatchPrinting.Commands.PauseBatchPrint;
using LASYS.Application.Features.BatchPrinting.Commands.ResumeBatchPrint;
using LASYS.Application.Features.BatchPrinting.Commands.StartBatchPrint;
using LASYS.Application.Features.BatchPrinting.Commands.StopBatchPrint;
using LASYS.Application.Features.BatchPrinting.Enums;
using LASYS.Application.Features.BatchPrinting.Events;
using LASYS.Application.Features.BatchPrinting.Models;
using LASYS.Application.Features.BatchPrinting.Services;
using LASYS.Application.Features.Devices.Events;
using LASYS.Application.Features.Devices.Models;
using LASYS.Application.Features.LabelInstructions.GetLabelInstructionContext;
using LASYS.Application.Features.LabelInstructions.GetWorkOrderListBySectionId;
using LASYS.Application.Features.PrintLabels.Helpers;
using LASYS.Application.Interfaces.Services;
using LASYS.Application.Interfaces.Services.Camera;
using LASYS.DesktopApp.Views.Interfaces;
using LASYS.Infrastructure.Hardware.Camera;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace LASYS.DesktopApp.Presenters
{
    public class LabelPrintingPresenter
    {
        public UserControl View { get; }
        private readonly IBatchPrintProcessService _batchPrintService;
        private readonly IMediator _mediator;
        private readonly IServiceProvider _services;
        private readonly ILabelPrintingView _view;
        private readonly IMainView _mainView;
        private readonly INiceLabelTemplateService _niceLabelTemplateService;
        private readonly IDeviceManager _deviceManager;

        private readonly CameraPreviewPresenter _cameraPreviewPresenter;
        private bool _isCameraVisible;

        private Guid _cameraSubId;
        private PrintJobStatus _printJobStatus;
        private Guid? _activePrintJobId;
        public LabelPrintingPresenter(IBatchPrintProcessService batchPrintService,
                                      IMediator mediator,
                                      IServiceProvider services,
                                      ILabelPrintingView view,
                                      IMainView mainView,
                                      INiceLabelTemplateService niceLabelTemplateService,
                                      IDeviceManager deviceManager,
                                      IFrameHub frameHub)
        {
            _batchPrintService = batchPrintService;
            _mediator = mediator;
            _services = services;
            _view = view;
            _mainView = mainView;
            _niceLabelTemplateService = niceLabelTemplateService;
            _deviceManager = deviceManager;

            View = (UserControl)view;


            // VIEW EVENTS
            _view.BackToWorkOrdersRequested += OnBackToWorkOrdersRequested;
            _view.PrintRequested += OnPrintRequested;
            _view.PausePrintingRequested += OnPausePrintingRequested;
            _view.ResumePrintingRequested += OnResumePrintingRequested;
            _view.StopPrintingRequested += OnStopPrintingRequested;

            // DEVICE EVENTS
            _deviceManager.DeviceStatusChanged += OnDeviceStatusChanged;
            SyncDeviceStates();

            // SERVICE EVENTS
            _batchPrintService.OperatorDecisionRequired += OnDecisionRequired;
            _batchPrintService.JobStateChanged += OnJobStateChanged;
            _batchPrintService.LogGenerated += OnLogGenerated;


            // CAMERA PREVIEW
            _cameraPreviewPresenter = services.GetRequiredService<CameraPreviewPresenter>();

            _view.SetCameraPreview(_cameraPreviewPresenter.View);

            _view.CameraPreviewRequested += OnCameraPreviewRequested;
        }

        private async void OnCameraPreviewRequested(object? sender, EventArgs e)
        {
            if (!_deviceManager.Camera.IsStreaming)
            {
                await _deviceManager.Camera.StartStreamingAsync(
                    () => _deviceManager.Camera.DefaultResolution);
            }
            _isCameraVisible = !_isCameraVisible;
            _view.ToggleCameraPreview(_isCameraVisible);
        }

        private void OnStartCameraPreviewRequested(object? sender, EventArgs e)
        {
            //_deviceManager.Camera.StartStreamingAsync(() => _deviceManager.Camera.DefaultResolution);
        }

        private void SyncDeviceStates()
        {
            foreach (Application.Features.Devices.Enums.DeviceType device in Enum.GetValues(typeof(Application.Features.Devices.Enums.DeviceType)))
            {
                var status = _deviceManager.GetLastStatus(device);
                if (status == null) continue;

                UpdateUI(status);
            }
        }
        private void UpdateUI(DeviceStatus status)
        {
            _view.InvokeOnUI(() =>
            {
                _view.UpdateDeviceStatus(status);
            });
        }
        private void OnDeviceStatusChanged(object? sender, DeviceStatusChangedEventArgs e)
        {
            UpdateUI(e.Status);
        }

        private void OnLogGenerated(object? sender, LogEventArgs e)
        {
            _view.InvokeOnUI(() => _view.AddLog(e.Type, DateTime.Now, e.Message));
        }

        //public async Task LoadCameraStatusAsync()
        //{
        //    var status = await _mediator.Send(new GetCameraStatusQuery());
        //    _view.InvokeOnUI(() => _view.UpdateCameraStatus(status.Message, status.Description));
        //}

        private void OnJobStateChanged(object? sender, PrintJobState e)
        {
            _view.InvokeOnUI(() => _view.SetPrintingState(e.Status));

            _view.InvokeOnUI(() => _view.UpdateProgress(e.PrintedCount, e.TotalQuantity));

            _view.InvokeOnUI(() => _view.UpdatePrintingResults(e.TargetQuantity, e.Context.PrintDetails!.SetNumber, e.Context.PrintDetails!.BatchNumber, e.DisplaySequence, e.RemainingQuantity, e.Context.PrintDetails!.TotalPrinted, e.Context.PrintDetails!.TotalPassed, e.Context.PrintDetails!.TotalFailed, e.Context.PrintDetails!.TotalSampled));
            
            if(e.Status is PrintJobStatus.Completed or PrintJobStatus.Stopped)
            {
                _view.InvokeOnUI(() => _view.UpdateQuantityControl(e));
            }

            _printJobStatus = e.Status;


        }
        private async void OnPrintRequested(object? sender, PrintRequestedEventArgs e)
        {
            if (_printJobStatus is PrintJobStatus.InProgress)
                return;

            var jobId = e.JobId;
            var quantityToPrint = e.Quantity;
            await _mediator.Send(new StartBatchPrintCommand(jobId, quantityToPrint));
            _activePrintJobId = jobId;
        }

        private async void OnPausePrintingRequested(object? sender, EventArgs e)
        {
            if (_activePrintJobId is null)
                return;

            await _mediator.Send(new PauseBatchPrintCommand(_activePrintJobId.Value));
        }

        private async void OnResumePrintingRequested(object? sender, EventArgs e)
        {
            if (_activePrintJobId is null)
                return;

            await _mediator.Send(new ResumeBatchPrintCommand(_activePrintJobId.Value));
        }

        private async void OnStopPrintingRequested(object? sender, EventArgs e)
        {
            if (_activePrintJobId is null)
                return;

            await _mediator.Send(new StopBatchPrintCommand(_activePrintJobId.Value));

            _activePrintJobId = null;
        }



        private void OnDecisionRequired(object? sender, OperatorDecisionRequiredEventArgs e)
        {

            try
            {
                _view.InvokeOnUI(() => _view.ToggleActivityLogs());

                _view.InvokeOnUI(() =>
                {
                    var errorPresenter = _services.GetRequiredService<ErrorPresenter>();
                    errorPresenter.View.MessageText = errorPresenter.GetErrorMessage(e);

                    errorPresenter.View.Configure(e.FailureType);

                    _view.ShowError(errorPresenter.View);
                });
            }
            finally
            {
                _view.InvokeOnUI(() => _view.ToggleActivityLogs());
            }

        }

        private void OnDecision(object? sender, StepResult e)
        {
            var service = _services.GetRequiredService<IBatchPrintProcessService>();
            service.SetUserDecision(e);

            if (sender is Form form)
            {
                form.Close();
            }
        }

        public async Task InitializeDataAsync(WorkOrderItem labelInstruction, BoxType boxType)
        {
            _view.InvokeOnUI(() => _view.SetPrintingState(PrintJobStatus.Initializing));

            var itemCode = labelInstruction.ItemCode;
            var lotNo = labelInstruction.LotNo;
            var masterLabelRevNumber = labelInstruction.MasterLabelRevNumber;

            var result = await _mediator.Send(new GetLabelInstructionContextQuery(itemCode, lotNo, masterLabelRevNumber, boxType));

            if (!result.IsSuccess)
            {
                _view.InvokeOnUI(() =>
                    _view.AddLog(MessageType.Error, DateTime.Now, $"Failed to retrieve label instruction context: {result.ErrorOrDefault}"));
                return;
            }

            var context = result.Value!;
            var masterLabel = context.MasterLabelDetails;
            var niceLabelPath = masterLabel?.FilePath;
            var niceLabelFile = masterLabel?.LabelFile;


            var sw = Stopwatch.StartNew();

            string filePath = NiceLabelFilePathBuilder.Build(itemCode, masterLabelRevNumber, boxType);
            bool isNiceLabelExist = !string.IsNullOrWhiteSpace(niceLabelPath) && File.Exists(niceLabelPath);

            if (isNiceLabelExist)
                await NiceLabelFilePathBuilder.CopyFileAsync(niceLabelPath!, filePath);
            else
                await NiceLabelFilePathBuilder.CreateFileAsync(filePath, niceLabelFile!);

            sw.Stop();

            _view.AddLog(MessageType.Info, DateTime.Now, $"Template saved in {sw.ElapsedMilliseconds}ms");

            var updatedContext = context with
            {
                MasterLabelDetails = masterLabel?.WithResolvedFilePath(filePath)
            };

             var printJobContext = await _mediator.Send(new InitializeBatchPrintCommand(updatedContext));

            _view.InvokeOnUI(() => _view.InitializePrintingContext(printJobContext));
            //_view.InvokeOnUI(() => _view.SetPrintingState(PrintJobStatus.Ready));

        }


        private void OnBackToWorkOrdersRequested(object? sender, EventArgs e)
        {
            if (_printJobStatus is PrintJobStatus.InProgress or PrintJobStatus.Paused)
            {
                _mainView.ShowNavigationBlocked("Cannot navigate while printing is in progress.");
                return;
            }
            _niceLabelTemplateService.CloseTemplate();

            var workOrdersPresenter = _services.GetRequiredService<WorkOrdersPresenter>();
            _mainView?.LoadView(workOrdersPresenter.View, true); //false always new
            _mainView?.SetActiveNavigation(_mainView.WorkOrdersNavItem);
        }

    }
}
