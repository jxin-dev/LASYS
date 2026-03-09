using LASYS.Application.Contracts;
using LASYS.DesktopApp.Events;
using DrawingSize = System.Drawing.Size;

namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface IVisionSettingsView
    {
        event EventHandler InitializeRequested;
        DrawingSize PictureBoxSize { get; }
        void InvokeOnUI(Action action);
        void ShowOCRRegion(Rectangle viewerRegion);
        void PreviewOCRRegion(Rectangle viewerRegion);
        void DisplayOCRResult(string result);
        void DisplayFrame(Bitmap bitmap);

        event EventHandler<int> FocusValueChanged;
        event EventHandler<CalibrationEventArgs> SaveCalibrationClicked;
        event EventHandler<ImageRegionEventArgs> ComputeImageRegionRequested;
        void FinishCalibration(string message, bool isError = false);

        event EventHandler<OCRCoordinatesEventArgs> OCRTriggered;

        event EventHandler<OCRCoordinatesEventArgs> OCRCalibrationPreview;

        event EventHandler LoadRegisteredOcrItemsRequested;
        void DisplayRegisteredOcrItems(OCRConfig config);
        void UpdateCoordinateFields(Rectangle imageRegion, DrawingSize imageSize);
        bool AskRestartConfirmation(string message, string title = "Restart Required");
        void SetCameraList(IEnumerable<string>? cameras);
        void SetCameraResolutions(IEnumerable<string>? resolution);
        CameraInfo? SelectedCamera { get; }
        event EventHandler<CameraSelectedEventArgs> CameraPreviewStateChanged;
        event EventHandler<CameraSavedEventArgs> CameraConfigurationSaved;
        event EventHandler LoadCamerasRequested;
        event EventHandler<string> CameraResolutionSelected;

        void SelectCamera(string cameraName, string resolution, int focus);
        void ShowCameraNotification(string message, string caption, bool isError = false);
    }
}
