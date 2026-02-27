using LASYS.DesktopApp.Views.Interfaces;

namespace LASYS.DesktopApp.Presenters
{
    public class IEndToEndTestPresenter
    {
        public UserControl View { get; }
        private readonly IEndToEndTestView _view;

        public IEndToEndTestPresenter(IEndToEndTestView view)
        {
            _view = view;
            View = (UserControl)view;
        }
    }
}
