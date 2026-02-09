using LASYS.Camera.Models;

namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface IWebCameraView
    {
        void InvokeOnUI(Action action);

        event EventHandler CameraPreviewStateChanged;
        event EventHandler CameraConfigurationSaved;

        void SetPreviewButtonEnabled(bool isEnabled);
        void SetSaveButtonVisibility(bool visible);
        void SetCameraList(IEnumerable<CameraInfo> cameras);

        CameraInfo? SelectedCamera { get; }
        void ShowMessage(string message, string title, MessageBoxIcon icon);
    }
}
