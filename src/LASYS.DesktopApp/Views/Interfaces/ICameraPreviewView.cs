namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface ICameraPreviewView
    {
        void DisplayFrame(Bitmap frame);
        void InvokeOnUI(Action action);
        UserControl View { get; }
    }
}
