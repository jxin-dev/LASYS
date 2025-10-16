using LASYS.DesktopApp.Presenters.Interfaces;

namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface ISplashView
    {
        void SetPresenter(ISplashPresenter presenter);
        void UpdateProgress(int percent, string message);
        void CloseView();
    }
}
