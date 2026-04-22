using LASYS.Application.Features.OCRCalibration.GetOcrSupportedItems;

namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface IOcrItemLookupView
    {
        event Func<Task> ViewLoaded;
        DialogResult ShowDialog();
        void DisplayItems(List<OcrSupportedItemDto> items);
        void ShowError(string message);
        event Action<OcrSupportedItemDto> ItemSelected;
        void Close();
    }
}
