using LASYS.DesktopApp.Core.Interfaces;

namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface ILoginView
    {
        event Func<object?, EventArgs, Task>? LoginClicked;
        string Username { get; }
        string Password { get; }
        string? SelectedEnvironment { get; }
        void ShowMessage(string errorMessage);
        void SetLoginEnabled(bool enabled);
        void SetStatus(string status);

        event Func<object?, EventArgs, Task>? CheckForUpdatesRequested;
        void ShowStatusMessage(string message, MessageBoxIcon icon);
        void SetDialogResult(DialogResult result);

    }
}
