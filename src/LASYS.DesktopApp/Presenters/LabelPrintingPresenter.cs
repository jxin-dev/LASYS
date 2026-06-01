using System.Diagnostics;
using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Messaging;
using LASYS.Application.Common.Results;
using LASYS.Application.Events;
using LASYS.Application.Features.BatchPrinting.Commands.PauseBatchPrint;
using LASYS.Application.Features.BatchPrinting.Commands.ResumeBatchPrint;
using LASYS.Application.Features.BatchPrinting.Commands.StartBatchPrint;
using LASYS.Application.Features.BatchPrinting.Commands.StopBatchPrint;
using LASYS.Application.Features.BatchPrinting.Enums;
using LASYS.Application.Features.BatchPrinting.Events;
using LASYS.Application.Features.BatchPrinting.Models;
using LASYS.Application.Features.BatchPrinting.Services;
using LASYS.Application.Features.Devices.Queries.GetCameraStatus;
using LASYS.Application.Features.LabelInstructions.GetLabelInstructionContext;
using LASYS.Application.Features.LabelInstructions.GetWorkOrderListBySectionId;
using LASYS.Application.Features.PrintLabels.Helpers;
using LASYS.Application.Interfaces.Services;
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
        private readonly IPrinterService _printerService;

        private Guid? _activePrintJobId;
        public LabelPrintingPresenter(IBatchPrintProcessService batchPrintService,
                                      IMediator mediator,
                                      IServiceProvider services,
                                      ILabelPrintingView view,
                                      IMainView mainView,
                                      INiceLabelTemplateService niceLabelTemplateService,
                                      IPrinterService printerService)
        {
            _batchPrintService = batchPrintService;
            _mediator = mediator;
            _services = services;
            _view = view;
            _mainView = mainView;
            _niceLabelTemplateService = niceLabelTemplateService;
            _printerService = printerService;

            View = (UserControl)view;



            // VIEW EVENTS
            _view.BackToWorkOrdersRequested += OnBackToWorkOrdersRequested;
            _view.PrintRequested += OnPrintRequested;
            _view.PausePrintingRequested += OnPausePrintingRequested;
            _view.ResumePrintingRequested += OnResumePrintingRequested;
            _view.StopPrintingRequested += OnStopPrintingRequested;
            // SERVICE EVENTS
            _batchPrintService.OperatorDecisionRequired += OnDecisionRequired;
            _batchPrintService.JobStateChanged += OnJobStateChanged;
            _batchPrintService.LogGenerated += OnLogGenerated;

            //_printerService.DeviceStatusChanged += OnDeviceStatusChanged;
        }

        private void OnLogGenerated(object? sender, LogEventArgs e)
        {
            _view.InvokeOnUI(() => _view.AddLog(e.Type, DateTime.Now, e.Message));
        }

        //private void OnDeviceStatusChanged(object? sender, DeviceStatusChangedEventArgs e)
        //{
        //    _view.InvokeOnUI(() =>
        //            _view.AddLog(MessageType.Info, DateTime.Now, e.Status.Message));

        //}

        public async Task LoadCameraStatusAsync()
        {
            var status = await _mediator.Send(new GetCameraStatusQuery());
            _view.InvokeOnUI(() => _view.UpdateCameraStatus(status.Message, status.Description));
        }

        private void OnJobStateChanged(object? sender, PrintJobState e)
        {
            _view.InvokeOnUI(() =>
            {
                _view.SetPrintingState(e.Status);
                _view.UpdateProgress(e.PrintedCount, e.TotalQuantity);
            });

            if (e.Status is PrintJobStatus.Completed or PrintJobStatus.Stopped or PrintJobStatus.Failed)
            {
                _activePrintJobId = null;
            }
        }
        private async void OnPrintRequested(object? sender, PrintRequestedEventArgs e)
        {
            var context = e.Context;
            var quantityToPrint = e.Quantity;
            _activePrintJobId = await _mediator.Send(new StartBatchPrintCommand(context, quantityToPrint));

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
            _view.InvokeOnUI(() =>
            {
                var errorPresenter = _services.GetRequiredService<ErrorPresenter>();
                var errorForm = errorPresenter.View;
                errorForm.MessageText = errorPresenter.GetErrorMessage(e);
                _view.ShowError(errorForm);
            });
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


            _view.InvokeOnUI(() =>
            {
                _view.LoadPrintingContext(updatedContext);
                _view.SetPrintingState(PrintJobStatus.Ready);
            });

            // Load Nicelabel Template
            _niceLabelTemplateService.LoadTemplate(filePath);

        }


        private void OnBackToWorkOrdersRequested(object? sender, EventArgs e)
        {
            if (_activePrintJobId != null)
            {
                _mainView.ShowNavigationBlocked("Cannot navigate while printing is in progress.");
                return;
            }

            var workOrdersPresenter = _services.GetRequiredService<WorkOrdersPresenter>();
            _mainView?.LoadView(workOrdersPresenter.View, true); //false always new
            _mainView?.SetActiveNavigation(_mainView.WorkOrdersNavItem);
        }

    }
}
