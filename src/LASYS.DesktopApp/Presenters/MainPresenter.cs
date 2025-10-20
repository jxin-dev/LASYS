using LASYS.DesktopApp.Presenters.Interfaces;
using LASYS.DesktopApp.Views.Interfaces;

namespace LASYS.DesktopApp.Presenters
{
    public class MainPresenter : IMainPresenter
    {
        private IMainView? _view;
        public void AttachView(IMainView view)
        {
            _view = view;
        }
    }
}
