using System.Diagnostics;
using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Mappings;
using LASYS.Application.Common.Messaging;
using LASYS.Application.Common.Utilities;
using LASYS.Application.Events;
using LASYS.Application.Features.BatchPrinting.Commands.InitializeBatchPrint;
using LASYS.Application.Features.BatchPrinting.Commands.PauseBatchPrint;
using LASYS.Application.Features.BatchPrinting.Commands.ResumeBatchPrint;
using LASYS.Application.Features.BatchPrinting.Commands.StartBatchPrint;
using LASYS.Application.Features.BatchPrinting.Commands.StopBatchPrint;
using LASYS.Application.Features.BatchPrinting.Enums;
using LASYS.Application.Features.BatchPrinting.Events;
using LASYS.Application.Features.BatchPrinting.Helpers;
using LASYS.Application.Features.BatchPrinting.Models;
using LASYS.Application.Features.BatchPrinting.Services;
using LASYS.Application.Features.Devices.Events;
using LASYS.Application.Features.Devices.Models;
using LASYS.Application.Features.LabelInstructions.GetLabelInstructionContext;
using LASYS.Application.Features.LabelInstructions.GetWorkOrderListBySectionId;
using LASYS.Application.Features.PrintLabels.Helpers;
using LASYS.Application.Interfaces.Services;
using LASYS.Application.Interfaces.Services.Camera;
using LASYS.Application.Interfaces.Services.NiceLabel;
using LASYS.DesktopApp.Views.Interfaces;
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
        private readonly ILabelPreviewHub _labelPreviewHub;

        private readonly CameraPreviewPresenter _cameraPreviewPresenter;
        private readonly LabelTemplatePreviewPresenter _labelTemplatePreviewPresenter;

        private bool _isCameraVisible;
        private bool _isLabelTemplateVisible;


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
                                      IFrameHub frameHub,
                                      ILabelPreviewHub labelPreviewHub)
        {
            _batchPrintService = batchPrintService;
            _mediator = mediator;
            _services = services;
            _view = view;
            _mainView = mainView;
            _niceLabelTemplateService = niceLabelTemplateService;
            _deviceManager = deviceManager;
            _labelPreviewHub = labelPreviewHub;


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
            _view.SetPreview(_cameraPreviewPresenter.View);
            _view.CameraPreviewRequested += OnCameraPreviewRequested;

            _labelTemplatePreviewPresenter = services.GetRequiredService<LabelTemplatePreviewPresenter>();
            _view.SetPreview(_labelTemplatePreviewPresenter.View);
            _view.LabelTemplatePreviewRequested += OnLabelTemplatePreviewRequested;
        }

        private void OnLabelTemplatePreviewRequested(object? sender, EventArgs e)
        {
            _isLabelTemplateVisible = !_isLabelTemplateVisible;
            if (_isLabelTemplateVisible)
            {
                _isCameraVisible = false;
                _view.ToggleCameraPreview(false);
            }
            _view.ToggleLabelTemplatePreview(_isLabelTemplateVisible);
        }

        private void OnCameraPreviewRequested(object? sender, EventArgs e)
        {
            _isCameraVisible = !_isCameraVisible;
            if (_isCameraVisible)
            {
                _isLabelTemplateVisible = false;
                _view.ToggleLabelTemplatePreview(false);
            }
            _view.ToggleCameraPreview(_isCameraVisible);
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

        private void OnJobStateChanged(object? sender, PrintJobState e)
        {
            _view.InvokeOnUI(() => _view.SetPrintingState(e.Status));

            _view.InvokeOnUI(() => _view.UpdateProgress(e.PrintedCount, e.TotalQuantity));

            _view.InvokeOnUI(() => _view.UpdatePrintingResults(e.TargetQuantity, e.Context.PrintDetails!.SetNumber, e.Context.PrintDetails!.BatchNumber, e.DisplaySequence, e.RemainingQuantity, e.Context.PrintDetails!.TotalPrinted, e.Context.PrintDetails!.TotalPassed, e.Context.PrintDetails!.TotalFailed, e.Context.PrintDetails!.TotalSampled));

            if (e.Status is PrintJobStatus.Completed or PrintJobStatus.Stopped)
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



        private async void OnDecisionRequired(object? sender, OperatorDecisionRequiredEventArgs e)
        {

            try
            {
                _view.InvokeOnUI(() => _view.ToggleActivityLogs());
                await Task.Delay(250);
                var errorPresenter = _services.GetRequiredService<ErrorPresenter>();
                errorPresenter.View.MessageText = errorPresenter.GetErrorMessage(e);
                errorPresenter.View.Configure(e.FailureType);
                _view.InvokeOnUI(() => _view.ShowError(errorPresenter.View));

            }
            finally
            {
                _view.InvokeOnUI(() => _view.ToggleActivityLogs());
            }

        }

        public async Task InitializeDataAsync(WorkOrderItem labelInstruction, BoxType boxType)
        {
            try
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
                // Display the label template in the preview
                DisplayTemplate(context);
            }
            finally
            {

            }

        }


        private void DisplayTemplate(LabelPrintingContext context)
        {
            try
            {
                var currentSequence = context.PrintDetails?.NextSequence ?? 0;
                var formattedCurrentSequence = SequenceFormatter.Format(currentSequence, 6);

                var labelData = NiceLabelDataMappings.ToLabelData(context).Clone().Add("BOX_NO", formattedCurrentSequence);

                var templateVariables = _niceLabelTemplateService.GetTemplateVariables().ToHashSet(StringComparer.OrdinalIgnoreCase);

                var variablesToSet = labelData.Variables.Where(x => templateVariables.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);

                _niceLabelTemplateService.SetVariables(variablesToSet);

                var sampleDirectory = PrintJobPathBuilder.CreateSampleDirectory(context.LabelInstructionDetails!.ItemCode);

                var filename = PrintFileNameBuilder.Build(context.LabelInstructionDetails!.ItemCode, context.LabelInstructionDetails.LotNo, formattedCurrentSequence);

                var imagePath = Path.Combine(sampleDirectory, $"{filename}.jpg");

                var isPreviewGenerated = _niceLabelTemplateService.GeneratePreview(sampleDirectory, filename);

                if (File.Exists(imagePath))
                {
                    _labelPreviewHub.Publish(imagePath);
                    OnLabelTemplatePreviewRequested(this, EventArgs.Empty);
                }

            }
            catch (Exception ex)
            {

            }
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
