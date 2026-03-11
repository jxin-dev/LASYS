using LASYS.Application.Common.Enums;
using LASYS.Application.Events;
using LASYS.Application.Features.LabelProcessing.Abstractions;
using LASYS.Application.Features.LabelProcessing.LoadLabelTemplate;
using LASYS.Application.Features.LabelProcessing.StartLabelJob;
using LASYS.DesktopApp.DTOs;
using LASYS.DesktopApp.Views.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

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
        public LabelPrintingPresenter(ILabelPrintingView view,
                                      IMainView mainView,
                                      IServiceProvider services,
                                      IMediator mediator,
                                      ILabelProcessingService labelProcessingService)
        {
            _view = view;
            _mainView = mainView;
            _services = services;
            _mediator = mediator;
            _labelProcessingService = labelProcessingService;

            View = (UserControl)view;

            _view.BackToWorkOrdersRequested += OnBackToWorkOrdersRequested;
            _view.PrintRequested += OnPrintRequested;
            _view.PausePrintingRequested += OnPausePrintingRequested;
            _view.ResumePrintingRequested += OnResumePrintingRequested;
            _view.StopPrintingRequested += OnStopPrintingRequested;

            _labelProcessingService.DecisionRequired += OnDecisionRequired;
            _labelProcessingService.LogGenerated += OnLogGenerated;
            _labelProcessingService.PrintControlsStateChanged += OnPrintControlsStateChanged;
        }

        private void OnPrintControlsStateChanged(object? sender, PrintingState e)
        {
            _view.InvokeOnUI(() => _view.SetPrintingState(e));
        }

        private void OnLogGenerated(object? sender, LogEventArgs e)
        {
            _view.InvokeOnUI(() => _view.AddLog(e.Type, e.Timestamp, e.Message));
        }

        public async Task InitializeTemplateAsync(int workOrderId)
        {
            var result = await _mediator.Send(new LoadLabelTemplateCommand(workOrderId));
            if (!result.IsSuccess)
            {
                //_view.ShowNotification(result.Error ?? "An unexpected error occurred.", "Error loading label template", MessageBoxIcon.Error);
                return;
            }

        }
        private void OnDecisionRequired(object? sender, OperatorDecisionRequiredEventArgs e)
        {
            var errorPresenter = _services.GetRequiredService<ErrorPresenter>();
            var errorForm = errorPresenter.View;
            errorForm.MessageText = errorPresenter.GetErrorMessage(e);

            _view.ShowError(errorForm);

            //var errorForm = _services.GetRequiredService<ErrorForm>();
            //_view.ShowError(errorForm);
        }

        private void OnBackToWorkOrdersRequested(object? sender, EventArgs e)
        {
            var workOrdersPresenter = _services.GetRequiredService<WorkOrdersPresenter>();
            _mainView?.LoadView(workOrdersPresenter.View);
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
            var command = new StartLabelJobCommand
            {
                ItemCode = "ITEM001",
                StartSequence = 1,
                Quantity = 100,
                SequenceVariableName = "SEQ",
                BarcodeVariableName = "BARCODE",
                ViewerSize = new Size(100, 100), //_view.PictureBoxSize,
                LabelData = new Dictionary<string, string>
                {
                    { "ItemCode", "ITEM001" }
                }
            };

            await _mediator.Send(command);

            //OnLabelOperationFailed(this, new LabelOperationFailedEventArgs(
            //    LabelOperationType.PrinterNotAvailable));

        }

        public void SetWorkOrderId(int workOrderId)
        {
            //Use repository to fetch work order data if needed using workOrderId and update the view.
            _view.UpdateWorkOrderData(new WorkOrderDto());
        }
    }
}
