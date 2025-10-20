using LASYS.DesktopApp.Core.Interfaces;

namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface ILoginView : IView
    {
        event EventHandler? LoginClicked;
        string Username { get; }
        string Password { get; }
        void ShowMessage(string errorMessage);
    }
}
