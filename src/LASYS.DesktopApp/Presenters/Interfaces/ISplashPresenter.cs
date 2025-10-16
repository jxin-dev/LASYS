using LASYS.DesktopApp.Views.Interfaces;

namespace LASYS.DesktopApp.Presenters.Interfaces
{
    public interface ISplashPresenter
    {
        void Initialize(ISplashView view);
        Task StartLoadingAsync();
    }
}
