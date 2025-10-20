using LASYS.DesktopApp.Core.Interfaces;
using LASYS.DesktopApp.Presenters.Interfaces;
using LASYS.DesktopApp.Views.Interfaces;

namespace LASYS.DesktopApp.Presenters
{
    public class LoginPresenter : ILoginPresenter
    {
        private ILoginView? _view;
        private readonly IViewFactory _factory;
        public LoginPresenter(IViewFactory factory)
        {
            _factory = factory;
        }

        public void AttachView(ILoginView view)
        {
            _view = view;
            _view.LoginClicked += OnLoginClicked;
        }

        private void OnLoginClicked(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_view?.Username) || string.IsNullOrWhiteSpace(_view?.Password))
            {
                _view?.ShowMessage("Please enter both username and password.");
                return;
            }

            if (_view!.Username == "admin" && _view.Password == "1234")
            {
                _view.HideView();
                var main = _factory.Create<IMainView, MainPresenter>();
                main.ShowView();
            }
            else
            {
                _view.ShowMessage("Invalid username or password.");
            }
        }
    }
}
