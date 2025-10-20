using LASYS.DesktopApp.Core.Interfaces;

namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface ISplashView : IView
    {
        void UpdateProgress(int percent, string message);
    }
}
