using LASYS.DesktopApp.Core.Interfaces;

namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface ISplashView
    {
        event EventHandler ViewShown;
        void UpdateProgress(int percent, string message);
        void CloseView();
        void InvokeOnUI(Action action);
        void ShowView();
    }
}
