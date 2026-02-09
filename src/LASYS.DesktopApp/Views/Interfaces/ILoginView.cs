using LASYS.DesktopApp.Core.Interfaces;

namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface ILoginView
    {
        event EventHandler LoginClicked;
        string Username { get; }
        string Password { get; }
        void ShowMessage(string errorMessage);


        event Func<object?, EventArgs, Task>? CheckForUpdatesRequested;
        void ShowStatusMessage(string message, MessageBoxIcon icon);
        void SetDialogResult(DialogResult result);
    }
}
