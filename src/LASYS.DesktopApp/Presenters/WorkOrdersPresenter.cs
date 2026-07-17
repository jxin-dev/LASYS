using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using LASYS.Application.Common.Enums;
using LASYS.Application.Features.BarcodeValidation;
using LASYS.Application.Features.BarcodeValidation.ValidateInstructionBarcode;
using LASYS.Application.Features.BarcodeValidation.ValidateLabelBarcode;
using LASYS.Application.Features.LabelInstructions.GetLabelInstructionsBySectionId;
using LASYS.Application.Features.LabelInstructions.GetWorkOrderListBySectionId;
using LASYS.Application.Interfaces.Services;
using LASYS.DesktopApp.Core.Interfaces;
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

        private List<WorkOrderItem> _workOrders = [];
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
            _view.LabelInstructionRequested += OnLabelInstructionRequested;

            _ = LoadWorkOrdersAsync();
        }
        private async void OnLabelInstructionRequested(object? sender, string e)
        {
            //e - barcode scanned text
            var barcodeScannedText = e;
            var result = await _mediator.Send(new ValidateInstructionBarcodeQuery(barcodeScannedText, false));
            if (!result.IsValid)
            {
                _view.ShowNotification(result.ErrorMessage, "Error", MessageBoxIcon.Error);
                return;
            }

            var barcodeWithBoxType = result.ApplicationIdentifiers.GetValueOrDefault("91");
            if(string.IsNullOrWhiteSpace(barcodeWithBoxType) || barcodeWithBoxType.Length != 14)
            {
                _view.ShowNotification("Invalid barcode format: missing box type information.", "Error", MessageBoxIcon.Error);
                return;
            }
            var boxTypeCode = barcodeWithBoxType.Substring(0, 2);
            BoxType boxType = boxTypeCode switch
            {
                "05" => BoxType.CartonBox,
                "09" => BoxType.AdditionalCartonBox,
                "07" => BoxType.OuterCartonBox,
                "03" => BoxType.UnitBox,
                "08" => BoxType.AdditionalUnitBox,
                "04" => BoxType.OuterUnitBox,
                _ => throw new InvalidOperationException($"Unknown box type: {boxTypeCode}")
            };

            var expiration = result.ApplicationIdentifiers.GetValueOrDefault("92");
            var lotNo = result.ApplicationIdentifiers.GetValueOrDefault("10");

            var barcodeNo = barcodeWithBoxType.Substring(2);

            var workOrder = _workOrders.FirstOrDefault(x =>
                string.Equals(x.LotNo, lotNo, StringComparison.OrdinalIgnoreCase) &&
                x.Details != null &&
                x.Details.TryGetValue(boxType, out var detail) &&
                string.Equals(detail.InstructionCode,
                              barcodeScannedText,
                              StringComparison.OrdinalIgnoreCase));

            if (workOrder is null)
            {
                _view.ShowNotification("No matching work order was found for the scanned barcode.", "Work Order Not Found", MessageBoxIcon.Exclamation);
                return;
            }

            var mainPresenter = _serviceProvider.GetRequiredService<MainPresenter>();
            mainPresenter.DisableNavigation();

            var labelPrintingPresenter = _serviceProvider.GetRequiredService<LabelPrintingPresenter>();
            _mainView?.LoadView(labelPrintingPresenter.View, false); //false always new
            await Task.Run(() => labelPrintingPresenter.InitializeDataAsync(workOrder, boxType));

           


            // Use workOrder
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

            //_ = labelPrintingPresenter.InitializeDataAsync(workOrder, result.Value);
            //_ = Task.Run(() => labelPrintingPresenter.InitializeDataAsync(workOrder, result.Value));
            Task.Run(() => labelPrintingPresenter.InitializeDataAsync(workOrder, result.Value));

            var mainPresenter = _serviceProvider.GetRequiredService<MainPresenter>();
            mainPresenter.DisableNavigation();
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
                        _workOrders = paginatedList.Items;
                        _view.InvokeOnUI(() => _view.SetWorkOrders(paginatedList.Items, paginatedList.TotalPages));
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
