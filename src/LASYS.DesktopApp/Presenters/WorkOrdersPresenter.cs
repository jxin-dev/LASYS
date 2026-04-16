using LASYS.Application.Features.LabelProcessing.GetWorkOrders;
using LASYS.DesktopApp.Core.Interfaces;
using LASYS.DesktopApp.Events;
using LASYS.DesktopApp.Views.Interfaces;
using LASYS.DesktopApp.Views.UserControls;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
            //Add logic to check if the item code has different label box types
            //use mediatr to send a query to get the box types for the item code
            //Get all the box types for the item code, then if there are a box type,
            //show a dialog to select the box type, then pass the selected box type to the label printing presenter

            var labelBoxTypePresenter = _serviceProvider.GetRequiredService<LabelBoxTypePresenter>();
            var result = labelBoxTypePresenter.Show(
                hasCaseLabel: true,
                hasAdditionalUnitBox: true,
                hasUnitBox: false,
                hasOuterUnitBox: true,
                hasCartonBox: false,
                hasOuterCartonBox: false,
                hasAdditionalCartonBox: true
                );
            if (result != null)
            {
                //Update this to pass the selected box type to the label printing presenter

                var labelPrintingPresenter = _serviceProvider.GetRequiredService<LabelPrintingPresenter>();
                _mainView?.LoadView(labelPrintingPresenter.View, false); //always new
                await labelPrintingPresenter.InitializeTemplateAsync(e.WorkOrderId);

            }

        }

        private async void OnPageNoChanged(object? sender, int pageNo)
        {
            if (_isLoading) return;

            var result = await _mediator.Send(new GetWorkOrdersQuery("", 50, pageNo));
            List<SampleData> data = result.Value.Items.Select(wo => new SampleData(
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
                _view.SetWorkOrders(data, result.Value.TotalPages);
            }
        }

        private async void LoadWorkOrdersAsync()
        {
            var result = await _mediator.Send(new GetWorkOrdersQuery("", 50, 1));
            List<SampleData> data = result.Value.Items.Select(wo => new SampleData(
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
                _view.SetWorkOrders(data, result.Value.TotalPages);
            }

            _isLoading = false;
        }
    }
}
