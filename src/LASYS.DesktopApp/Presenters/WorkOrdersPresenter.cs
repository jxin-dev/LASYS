using System.Threading.Tasks;
using LASYS.Application.Features.LabelProcessing.LoadLabelTemplate;
using LASYS.DesktopApp.Events;
using LASYS.DesktopApp.Views.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace LASYS.DesktopApp.Presenters
{
    public class WorkOrdersPresenter
    {
        private readonly IWorkOrdersView _view;
        private readonly IMainView _mainView;
        private readonly IServiceProvider _services;
        private readonly IMediator _mediator;

        public UserControl View { get; }
        public WorkOrdersPresenter(IWorkOrdersView view, IMainView mainView, IServiceProvider services, IMediator mediator)
        {
            View = (UserControl)view;
            _view = view;
            _mainView = mainView;
            _services = services;
            _mediator = mediator;

            _view.LabelPrintingRequested += OnLabelPrintingRequested;
        }

        private async void OnLabelPrintingRequested(object? sender, LabelPrintingRequestedEventArgs e)
        {
            var labelPrintingPresenter = _services.GetRequiredService<LabelPrintingPresenter>();

            labelPrintingPresenter.SetWorkOrderId(e.WorkOrderId);

            _mainView?.LoadView(labelPrintingPresenter.View);

            await labelPrintingPresenter.InitializeTemplateAsync(e.WorkOrderId);
        }
    }
}
