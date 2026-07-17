using LASYS.Application.Features.BatchPrinting.Commands.SetBatchPrintDecision;
using LASYS.Application.Features.BatchPrinting.Enums;
using LASYS.Application.Features.BatchPrinting.Events;
using LASYS.Application.Features.BatchPrinting.Services;
using LASYS.DesktopApp.Views.Forms;
using LASYS.DesktopApp.Views.Interfaces;
using MediatR;

namespace LASYS.DesktopApp.Presenters
{
    public class ErrorPresenter
    {
        public ErrorForm View { get; }
        private readonly IErrorView _view;
        private readonly ILabelPrintingView _labelPrintingView;
        private readonly IMediator _mediator;
        private readonly IBatchPrintProcessService _batchPrintService;
        public event EventHandler<StepResult>? ApprovalRequired;
        public ErrorPresenter(IErrorView view, IMediator mediator, IBatchPrintProcessService batchPrintService, ILabelPrintingView labelPrintingView)
        {
            _view = view;
            _labelPrintingView = labelPrintingView;
            _mediator = mediator;
            _batchPrintService = batchPrintService;

            View = (ErrorForm)view;

            _view.DecisionRequested += OnDecisionRequested;
        }

        private async void OnDecisionRequested(object? sender, StepResult e)
        {
            switch (e)
            {
                case StepResult.Retry:

                    await _mediator.Send(new SetBatchPrintDecisionCommand(e));

                    _view.InvokeOnUI(() => _view.CloseError());
                    _view.InvokeOnUI(() => _labelPrintingView.HideModal());
                    break;

                case StepResult.Skip:
                case StepResult.Stop:
                    _view.InvokeOnUI(() => _view.HideError());

                    var approval = await _batchPrintService.RequestApprovalAsync(default);

                    if (!approval.IsApproved)
                    {
                        _view.InvokeOnUI(() => _view.ShowError());
                        return;
                    }

                    await _mediator.Send(new SetBatchPrintDecisionCommand(e));
                    _view.InvokeOnUI(() => _view.CloseError());
                    _view.InvokeOnUI(() => _labelPrintingView.HideModal());


                    break;
            }

            //await _mediator.Send(new SetBatchPrintDecisionCommand(e));
            //_view.CloseError();
        }

        public string GetErrorMessage(OperatorDecisionRequiredEventArgs e)
        {
            return e.GetMessage();
        }

    }
}
