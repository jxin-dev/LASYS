using LASYS.Application.Features.LabelProcessing.GetWorkOrders;
using LASYS.DesktopApp.Events;
using LASYS.DesktopApp.Views.Interfaces;
using LASYS.DesktopApp.Views.UserControls;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace LASYS.DesktopApp.Presenters
{
    public class WorkOrdersPresenter
    {
        private readonly IWorkOrdersView _view;
        private readonly IMainView _mainView;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMediator _mediator;
        public UserControl View { get; }

        public WorkOrdersPresenter(IWorkOrdersView view, IMainView mainView, IServiceProvider serviceProvider, IMediator mediator)
        {
            View = (UserControl)view;
            _view = view;
            _mainView = mainView;
            _serviceProvider = serviceProvider;
            _mediator = mediator;

            _view.LabelPrintingRequested += OnLabelPrintingRequested;

            LoadWorkOrdersAsync();
        }


        private async void OnLabelPrintingRequested(object? sender, LabelPrintingRequestedEventArgs e)
        {
            var labelPrintingPresenter = _serviceProvider.GetRequiredService<LabelPrintingPresenter>();
            //labelPrintingPresenter.SetWorkOrderId(e.WorkOrderId);
            _mainView?.LoadView(labelPrintingPresenter.View, false); //always new
            await labelPrintingPresenter.InitializeTemplateAsync(e.WorkOrderId);


        }

        private async void LoadWorkOrdersAsync()
        {
            var result = await _mediator.Send(new GetWorkOrdersQuery("", 10, 1));
            List<SampleData> data = result.Value.Select(wo => new SampleData(
                1,
                wo.ItemCode,
                wo.LotNo,
                wo.ExpDate,
                wo.PrintType,
                wo.Verdict,
                wo.DateApproved,
                wo.ProdQty,
                wo.MasterLabelRevisionNo,
                wo.LabelInsRevisionNo,
                wo.UB_Qty.ToString(),
                wo.UB_LI_Status,
                wo.AUB_Qty.ToString(),
                wo.AUB_LI_Status,
                wo.OUB_Qty.ToString(),
                wo.OUB_LI_Status
            )).ToList();

            if (data != null && data.Count > 0)
            {
                _view.SetWorkOrders(data);
            }
        }
    }
}
