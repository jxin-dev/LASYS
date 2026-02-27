using LASYS.Application.Contracts;
using LASYS.DesktopApp.Events;

namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface IWebCameraView
    {
        void InvokeOnUI(Action action);
        void DisplayFrame(Bitmap bitmap);
        void ShowMessage(string message);

        event EventHandler<CameraSelectedEventArgs> CameraPreviewStateChanged;
        event EventHandler<CameraSavedEventArgs> CameraConfigurationSaved;

        bool AskRestartConfirmation(string message, string title = "Restart Required");

        void SetCameraList(IEnumerable<CameraInfo> cameras);
        void SetCameraResolutions(IEnumerable<string> resolution);

        CameraInfo? SelectedCamera { get; }
        void ShowMessage(string message, string title, MessageBoxIcon icon);
    }
}
