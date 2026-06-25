using LASYS.Application.Features.OCRCalibration.GetOcrSupportedItems;
using LASYS.DesktopApp.Events;

namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface IOcrItemLookupView
    {
        event Func<Task> ViewLoaded;
        DialogResult ShowDialog();
        void DisplayItems(List<OcrSupportedItemDto> items, int totalPages);
        event EventHandler<SampleLabelPrintingRequestedEventArgs> LabelPrintingRequested;

        void InvokeOnUI(Action action);
        void ShowError(string message);
        void ShowWarning(string message);

        event Action<OcrSupportedItemDto> ItemSelected;
        void Close();
    }
}
