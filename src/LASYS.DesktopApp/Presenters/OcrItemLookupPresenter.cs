using LASYS.Application.Common.Enums;
using LASYS.Application.Features.OCRCalibration.GetOcrSupportedItems;
using LASYS.DesktopApp.Events;
using LASYS.DesktopApp.Views.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace LASYS.DesktopApp.Presenters
{
    public class OcrItemLookupPresenter
    {
        private readonly IOcrItemLookupView _view;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMediator _mediator;

        private (string ItemCode, uint MasterLabelRevNumber, string BoxType) _selectedItem;
        public OcrItemLookupPresenter(IOcrItemLookupView view, IMediator mediator, IServiceProvider serviceProvider)
        {
            _view = view;
            _mediator = mediator;
            _serviceProvider = serviceProvider;

            _view.ViewLoaded += LoadItemsAsync;
            //_view.ItemSelected += OnItemSelected;
            _view.LabelPrintingRequested += OnLabelPrintingRequested;
        }

        private void OnLabelPrintingRequested(object? sender, SampleLabelPrintingRequestedEventArgs e)
        {
            var labelBoxTypePresenter = _serviceProvider.GetRequiredService<LabelBoxTypePresenter>();
            var item = e.Item;
            if (string.IsNullOrWhiteSpace(item.ItemCode))
            {
                //_view.ShowNotification("Item Code or Lot Number is missing.", "Error", MessageBoxIcon.Error);
                return;
            }

            var result = labelBoxTypePresenter.Show(item.AvailableBoxTypes);

            if (result is null)
                return;

            _selectedItem = (e.Item.ItemCode, e.Item.MasterLabelRevNumber, GetBoxTypeCode(result.Value));
            _view.Close();
        }
        private static string GetBoxTypeCode(BoxType boxType) => boxType switch
        {
            BoxType.CaseLabel => "CASE",
            BoxType.UnitBox => "UB",
            BoxType.AdditionalUnitBox => "AUB",
            BoxType.OuterUnitBox => "OUB",
            BoxType.CartonBox => "CB",
            BoxType.AdditionalCartonBox => "ACB",
            BoxType.OuterCartonBox => "OCB",
            _ => throw new ArgumentOutOfRangeException(nameof(boxType))
        };
        //private void OnItemSelected(OcrSupportedItemDto item)
        //{
        //    //if (item.FilePath == "NO FILE PATH")
        //    //{
        //    //    _view.ShowError("The selected item does not have a valid file path. Please select another item.");
        //    //    return;
        //    //}
        //    _selectedItem = item;
        //    _view.Close();
        //}

        public (string ItemCode, uint MasterLabelRevNumber, string BoxType) Show()
        {
            _view.ShowDialog();
            return _selectedItem;
        }

        public async Task LoadItemsAsync()
        {
            var result = await _mediator.Send(new GetOcrSupportedItemsQuery());

            if (!result.IsSuccess)
            {
                _view.ShowError(result.ErrorOrDefault);
                return;
            }
            var paginatedList = result.Value;
            if (paginatedList?.Items != null && paginatedList.Items.Count > 0)
            {
                _view.InvokeOnUI(() => _view.DisplayItems(paginatedList.Items, paginatedList.TotalPages));
            }
        }
    }
}
