using LASYS.DesktopApp.Core.Interfaces;
using LASYS.DesktopApp.Presenters.Interfaces;
using LASYS.DesktopApp.Views.Interfaces;

namespace LASYS.DesktopApp.Presenters
{
    public class SplashPresenter : ISplashPresenter
    {
        private ISplashView? _view;
        private readonly IViewFactory _factory;

        public SplashPresenter(IViewFactory factory)
        {
            _factory = factory;
        }

        public void AttachView(ISplashView view)
        {
            _view = view;
        }
        public async Task InitializeAsync()
        {
            _view?.UpdateProgress(0, "Loading, Please wait...");
            await Task.Delay(500);

            _view?.UpdateProgress(10, "Checking for updates...");
            await Task.Delay(500);

            _view?.UpdateProgress(15, "Connecting to camera...");
            await Task.Delay(500);

            _view?.UpdateProgress(100, "Launching application...");
            await Task.Delay(1000);

            _view?.HideView();

            var loginView = _factory.Create<ILoginView, LoginPresenter>();
            loginView.ShowView();
           
        }
    }
}
