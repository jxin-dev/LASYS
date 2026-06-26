namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface ICameraPreviewView
    {
        void DisplayFrame(Bitmap frame);
        void InvokeOnUI(Action action);
        UserControl View { get; }
        void ShowOcrRegion(Rectangle region);
        void HideOcrRegion();
        Size PictureBoxSize { get; }
    }
}
