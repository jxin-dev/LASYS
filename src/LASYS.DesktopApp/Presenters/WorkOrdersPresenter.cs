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
        private bool _isLoading = true;

        public WorkOrdersPresenter(IWorkOrdersView view, IMainView mainView, IServiceProvider serviceProvider, IMediator mediator)
        {
            View = (UserControl)view;
            _view = view;
            _mainView = mainView;
            _serviceProvider = serviceProvider;
            _mediator = mediator;

            _view.LabelPrintingRequested += OnLabelPrintingRequested;
            _view.PageNoChanged += OnPageNoChanged; 

            LoadWorkOrdersAsync();
        }


        private async void OnLabelPrintingRequested(object? sender, LabelPrintingRequestedEventArgs e)
        {
            var labelPrintingPresenter = _serviceProvider.GetRequiredService<LabelPrintingPresenter>();

            // Populate view with basic/placeholder data before loading (so controls aren't empty)
            try
            {
                // Pass the full event args so the presenter can use ItemCode / LotNo / InstructionCode / LabelType
                labelPrintingPresenter.SetWorkOrder(e);
            }
            catch { }

            _mainView?.LoadView(labelPrintingPresenter.View, false); //always new

            // Initialize template and other data using the full event args
            await labelPrintingPresenter.InitializeTemplateAsync(e);
        }

        private async void OnPageNoChanged(object? sender, int pageNo)
        {
            if (_isLoading) return;

            var result = await _mediator.Send(new GetWorkOrdersQuery("", 50, pageNo));

            if(result.IsSuccess && result.Value?.Items == null) return;                                     

            List<SampleData> data = result.Value!.Items.Select((wo, index) => new SampleData(
                index + 1,
                wo.ItemCode!,
                wo.LotNo!,                                                                                                                                                           
                wo.ExpDate!,
                wo.PrintType!,
                wo.Verdict!,
                wo.DateApproved!,
                wo.ProdQty,
                wo.MasterLabelRevisionNo,
                wo.LabelInsRevisionNo,
                wo.UB_Qty.ToString(), wo.UB_LI_Status!,
                wo.AUB_Qty.ToString(), wo.AUB_LI_Status!,
                wo.OUB_Qty.ToString(), wo.OUB_LI_Status!
            )).ToList();
            if (data != null && data.Count > 0)
            {
                _view.SetWorkOrders(data, result.Value.TotalPages);
            }
        }

        private async void LoadWorkOrdersAsync()
        {
            var result = await _mediator.Send(new GetWorkOrdersQuery("", 50, 1));
            List<SampleData> data = result.Value!.Items.Select((wo, index) => new SampleData(
                index + 1,
                wo.ItemCode!,
                wo.LotNo!,
                wo.ExpDate!,
                wo.PrintType!,
                wo.Verdict!,
                wo.DateApproved!,
                wo.ProdQty,
                wo.MasterLabelRevisionNo,
                wo.LabelInsRevisionNo,
                wo.UB_Qty.ToString(), wo.UB_LI_Status!,
                wo.AUB_Qty.ToString(), wo.AUB_LI_Status!,
                wo.OUB_Qty.ToString(), wo.OUB_LI_Status!
            )).ToList();

            if (data != null && data.Count > 0)
            {
                _view.SetWorkOrders(data, result.Value.TotalPages);
            }

            _isLoading = false;
        }
    }
}
