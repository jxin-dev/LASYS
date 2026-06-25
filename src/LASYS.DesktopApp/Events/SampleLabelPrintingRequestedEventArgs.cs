using LASYS.Application.Features.OCRCalibration.GetOcrSupportedItems;

namespace LASYS.DesktopApp.Events
{
    public class SampleLabelPrintingRequestedEventArgs : EventArgs
    {
        public OcrSupportedItemDto Item { get; }
        public SampleLabelPrintingRequestedEventArgs(OcrSupportedItemDto item)
        {
            Item = item;
        }
    }
}
