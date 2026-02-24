using LASYS.OCR.Models;
using LASYS.DesktopApp.Events;
using DrawingSize = System.Drawing.Size;

namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface IOCRCalibrationView
    {

        event EventHandler InitializeRequested;
        event EventHandler StreamingRequested;


        void SetReconnectCameraButtonVisibility(bool isVisible);

        DrawingSize PictureBoxSize { get; }
        void InvokeOnUI(Action action);
        void ShowCameraStatus(string message, bool isError = false);
        void ShowPrinterStatus(string message);
        void ShowBarcodeStatus(string message, bool isError = false);


        void ShowOCRRegion(Rectangle viewerRegion);

        void ShowOCRResult(string result, string msg, bool isSuccess = true);

        void DisplayFrame(Bitmap bitmap);



        event EventHandler<CalibrationEventArgs> SaveCalibrationClicked;
        event EventHandler<ImageRegionEventArgs> ComputeImageRegionRequested;
        void FinishCalibration(string message, bool isError = false);

        event EventHandler<OCRCoordinatesEventArgs> OCRTriggered;

        event EventHandler LoadRegisteredOcrItemsRequested;
        void DisplayRegisteredOcrItems(OCRConfig config);
        void UpdateCoordinateFields(Rectangle imageRegion, DrawingSize imageSize);


    }
}
