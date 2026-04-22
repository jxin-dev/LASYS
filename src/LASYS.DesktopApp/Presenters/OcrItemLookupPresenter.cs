using LASYS.Application.Features.OCRCalibration.GetOcrSupportedItems;
using LASYS.DesktopApp.Views.Interfaces;
using MediatR;

namespace LASYS.DesktopApp.Presenters
{
    public class OcrItemLookupPresenter
    {
        private readonly IOcrItemLookupView _view;
        private readonly IMediator _mediator;

        private OcrSupportedItemDto? _selectedItem;
        public OcrItemLookupPresenter(IOcrItemLookupView view, IMediator mediator)
        {
            _view = view;
            _mediator = mediator;
            _view.ViewLoaded += LoadItemsAsync;
            _view.ItemSelected += OnItemSelected;
        }

        private void OnItemSelected(OcrSupportedItemDto item)
        {
            if (item.FilePath == "NO FILE PATH")
            {
                _view.ShowError("The selected item does not have a valid file path. Please select another item.");
                return;
            }
            _selectedItem = item;
            _view.Close();
        }

        public OcrSupportedItemDto? Show()
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

            _view.DisplayItems(result.Value!);
        }
    }
}
