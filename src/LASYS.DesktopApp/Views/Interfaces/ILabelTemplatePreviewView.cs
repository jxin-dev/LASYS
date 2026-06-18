namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface ILabelTemplatePreviewView
    {
        UserControl View { get; }
        void DisplayTemplate(Bitmap frame);
        void InvokeOnUI(Action action);
    }
}
