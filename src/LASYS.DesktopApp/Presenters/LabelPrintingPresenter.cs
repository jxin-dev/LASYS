using System.Drawing;
using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Messaging;
using LASYS.Application.Events;
using LASYS.Application.Features.LabelProcessing.Abstractions;
using LASYS.Application.Features.LabelProcessing.GetLabelTemplate;
using LASYS.Application.Features.LabelProcessing.StartLabelJob;
using LASYS.Application.Features.LabelProcessing.Contracts;
using LASYS.DesktopApp.DTOs;
using LASYS.DesktopApp.Views.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using LASYS.DesktopApp.Events;
using LASYS.Application.Common.Models;
using LASYS.Application.Interfaces.Services;
using LASYS.DesktopApp.State.Printing;

namespace LASYS.DesktopApp.Presenters
{
    public class LabelPrintingPresenter
    {
        public UserControl View { get; }
        private readonly ILabelPrintingView _view;
        private readonly IMainView _mainView;
        private readonly IServiceProvider _services;
        private readonly IMediator _mediator;
        private readonly ILabelProcessingService _labelProcessingService;
        private readonly IPrinterService _printerService;
        private readonly IPrintingState _printingState;

        private PrintData _printData;

        public LabelPrintingPresenter(ILabelPrintingView view,
                                      IMainView mainView,
                                      IServiceProvider services,
                                      IMediator mediator,
                                      ILabelProcessingService labelProcessingService,
                                      IPrinterService printerService,
                                      IPrintingState printingState)
        {
            _view = view;
            _mainView = mainView;
            _services = services;
            _mediator = mediator;
            _labelProcessingService = labelProcessingService;

            View = (UserControl)view;

            _printerService = printerService;
            _printingState = printingState;


            _view.BackToWorkOrdersRequested += OnBackToWorkOrdersRequested;
            _view.PrintRequested += OnPrintRequested;
            _view.PausePrintingRequested += OnPausePrintingRequested;
            _view.ResumePrintingRequested += OnResumePrintingRequested;
            _view.StopPrintingRequested += OnStopPrintingRequested;

            _labelProcessingService.DecisionRequired += OnDecisionRequired;
            _labelProcessingService.LogGenerated += OnLogGenerated;
            _labelProcessingService.PrintControlsStateChanged += OnPrintControlsStateChanged;
            _labelProcessingService.DeviceStatusChanged += OnDeviceStatusChanged;
         
        }

        private void OnDeviceStatusChanged(object? sender, DeviceStatusEventArgs e)
        {
            switch (e.Device)
            {
                case DeviceType.Camera:
                    _view.InvokeOnUI(() => _view.UpdateCameraStatus(e.Message, e.Description));
                    break;
                case DeviceType.Printer:
                    _view.InvokeOnUI(() => _view.UpdatePrinterStatus(e.Message, e.Description));
                    break;
                case DeviceType.Barcode:
                    _view.InvokeOnUI(() => _view.UpdateBarcodeStatus(e.Message, e.Description));
                    break;
                default:
                    break;
            }
        }

        private void OnPrintControlsStateChanged(object? sender, PrintJobState e)
        {
            _view.InvokeOnUI(() => _view.SetPrintingState(e));
        }

        private void OnLogGenerated(object? sender, LogEventArgs e)
        {
            _view.InvokeOnUI(() => _view.AddLog(e.Type, e.Timestamp, e.Message));
        }

        public void InitializePrintData(PrintData printData)
        {
            _printData = printData;

            _view.InvokeOnUI(() =>
            {
                _view.UpdateWorkOrderData(new WorkOrderDto
                {
                    LabelInstruction = new LabelInstruction(
                        InstructionCode: printData.InstructionCode,
                        ItemCode: printData.ItemCode,
                        ExpiryDate: printData.ExpiryDate,
                        LotNo: printData.LotNo,
                        LabelFile: string.Empty
                    ),
                    BarcodeLabel = new BarcodeLabel((int)printData.ProductQuantity),
                    BatchInformation = new BatchInformation(),
                    PrintingResultInformation = new PrintingResultInformation()
                });
            });
        }



        public async Task InitializeTemplateAsync(int workOrderId)
        {
            foreach (var status in _labelProcessingService.GetCurrentDeviceStatuses())
            {
                OnDeviceStatusChanged(this, status);
            }
            var result = await _mediator.Send(new GetLabelTemplateQuery(workOrderId));

            if (!result.IsSuccess)
            {
                OnLogGenerated(this, new LogEventArgs(MessageType.Error, result.ErrorOrDefault));
            }

            _labelProcessingService.LoadLabelTemplateAsync(result.Value!);

            _view.UpdateFilePath(result.Value!);
        }

        // New overload: accept full event args, populate view first then load template
        public async Task InitializeTemplateAsync(LabelPrintingRequestedEventArgs args)
        {
            // Populate view with initial info from event so controls show immediately
            try
            {
                SetWorkOrder(args);
            }
            catch { }

            // Call existing template initializer using workOrderId
            await InitializeTemplateAsync(args.WorkOrderId);
        }

        private void OnDecisionRequired(object? sender, OperatorDecisionRequiredEventArgs e)
        {
            var errorPresenter = _services.GetRequiredService<ErrorPresenter>();
            var errorForm = errorPresenter.View;
            errorForm.MessageText = errorPresenter.GetErrorMessage(e);

            _view.ShowError(errorForm);
        }

        private void OnBackToWorkOrdersRequested(object? sender, EventArgs e)
        {
            if (_printingState.Status is PrintingStatus.Printing or PrintingStatus.Paused)
            {
                _mainView.ShowNavigationBlocked("Cannot navigate while printing is in progress.");
                return;
            }

            var workOrdersPresenter = _services.GetRequiredService<WorkOrdersPresenter>();
            _mainView?.LoadView(workOrdersPresenter.View, false); //always new
            _mainView?.SetActiveNavigation(_mainView.WorkOrdersNavItem);
        }

        private void OnStopPrintingRequested(object? sender, EventArgs e)
        {
            _labelProcessingService.Stop();
        }

        private void OnResumePrintingRequested(object? sender, EventArgs e)
        {
            _labelProcessingService.Resume();
        }

        private void OnPausePrintingRequested(object? sender, EventArgs e)
        {
            _labelProcessingService.Pause();
        }

        private async void OnPrintRequested(object? sender, EventArgs e)
        {
            _printingState.SetPrinting(PrintingStatus.Printing);
            try
            {
                if (_printData == null)
                {
                    _view.InvokeOnUI(() => _view.AddLog(MessageType.Error, DateTime.Now, "Print data is not initialized."));
                    return;
                }

                var quantity = (int)Math.Max(1, _printData.TargetQuantity - _printData.TotalPassed);
                var request = new StartLabelJobRequest
                {
                    ItemCode = _printData.ItemCode,
                    StartSequence = (int)Math.Max(1, _printData.SequenceNumber),
                    Quantity = quantity,
                    SequenceVariableName = "SEQ",
                    BarcodeVariableName = "BARCODE",
                    SequencePaddingLength = 6,
                    LabelData = new Dictionary<string, string>
                    {
                        { "ItemCode", _printData.ItemCode },
                        { "InstructionCode", _printData.InstructionCode },
                        { "LotNo", _printData.LotNo },
                        { "ExpiryDate", _printData.ExpiryDate == DateTime.MinValue ? string.Empty : _printData.ExpiryDate.ToString("yyyy-MM-dd") },
                        { "LabelType", _printData.LabelType.ToString() }
                    }
                };

                // Start job via service
                await _labelProcessingService.StartJobAsync(new Size(800, 600), request);
            }
            catch (Exception ex)
            {
                _view.InvokeOnUI(() => _view.AddLog(MessageType.Error, DateTime.Now, ex.Message));
            }
        }

        // New: populate view from event args
        public void SetWorkOrder(LabelPrintingRequestedEventArgs args)
        {
            var labelInstruction = new LabelInstruction(
                InstructionCode: args.InstructionCode ?? string.Empty,
                ItemCode: args.ItemCode ?? string.Empty,
                ExpiryDate: null,
                LotNo: args.LotNo ?? string.Empty,
                LabelFile: string.Empty
            );

            var dto = new WorkOrderDto
            {
                LabelInstruction = labelInstruction,
                BarcodeLabel = new BarcodeLabel(),
                BatchInformation = new BatchInformation(),
                PrintingResultInformation = new PrintingResultInformation()
            };

            _view.InvokeOnUI(() => _view.UpdateWorkOrderData(dto));
        }

        public void SetWorkOrderId(int workOrderId)
        {
            _view.UpdateWorkOrderData(new WorkOrderDto());
        }
    }
}
