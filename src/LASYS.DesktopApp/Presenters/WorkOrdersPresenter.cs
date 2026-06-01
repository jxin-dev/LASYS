using LASYS.Application.Common.Enums;
using LASYS.Application.Features.LabelInstructions.GetLabelInstructionsBySectionId;
using LASYS.Application.Features.WorkOrders.GetWorkOrdersBySectionId;
using LASYS.Application.Interfaces.Services;
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
        private readonly IServiceProvider _serviceProvider;
        private readonly IMediator _mediator;
        private readonly ILogService _logService;
        public UserControl View { get; }
        private bool _isLoading;

        public WorkOrdersPresenter(IWorkOrdersView view,
                                   IMainView mainView,
                                   IServiceProvider serviceProvider,
                                   IMediator mediator,
                                   ILogService logService)
        {
            View = (UserControl)view;
            _view = view;
            _mainView = mainView;
            _serviceProvider = serviceProvider;
            _mediator = mediator;
            _logService = logService;

            _view.LabelPrintingRequested += OnLabelPrintingRequested;

            _ = LoadWorkOrdersAsync();
        }

        private void OnLabelPrintingRequested(object? sender, LabelPrintingRequestedEventArgs e)
        {
            var labelBoxTypePresenter = _serviceProvider.GetRequiredService<LabelBoxTypePresenter>();
            var workOrder = e.WorkOrderItem;
            if (string.IsNullOrWhiteSpace(workOrder.ItemCode) || string.IsNullOrWhiteSpace(workOrder.LotNo))
            {
                _view.ShowNotification("Item Code or Lot Number is missing.", "Error", MessageBoxIcon.Error);
                return;
            }

            var result = labelBoxTypePresenter.Show(workOrder.AvailableBoxTypes);

            if (result is null)
                return;
            
            var labelPrintingPresenter = _serviceProvider.GetRequiredService<LabelPrintingPresenter>();
            _mainView?.LoadView(labelPrintingPresenter.View, false); //false always new

            _ = labelPrintingPresenter.InitializeDataAsync(workOrder, result.Value);
        }

        private async Task LoadWorkOrdersAsync()
        {
            if (_isLoading)
                return;

            _isLoading = true;
            _view.SetLoading(true);

           //await PrintJobCleanup.DeleteOldFilesByHoursAsync(TimeSpan.FromMinutes(5));


            try
            {
                //
                var result = await _mediator.Send(new GetWorkOrderListBySectionIdQuery());
                if (result.IsSuccess)
                {
                    var paginatedList = result.Value;
                    if (paginatedList?.Items != null && paginatedList.Items.Count > 0)
                    {
                        _view.SetWorkOrders(paginatedList.Items, paginatedList.TotalPages);
                    }
                }
            }
            finally
            {
                _isLoading = false;
                _view.SetLoading(false);
            }
        }
    }
}
