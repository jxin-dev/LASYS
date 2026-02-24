namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface IBarcodeDeviceSetupView
    {
        void InvokeOnUI(Action action);

        event EventHandler LoadRequested;
        event EventHandler SaveClicked;
        event EventHandler SetManualModeClicked;
        void DisplayBarcodeStatus(string message, bool isError = false);
        void ShowNotification(string message, string caption, MessageBoxIcon icon);
        void SetUSBVirtualCOMPortList(IReadOnlyList<string> ports);
        void SetSelectedPort(string port);
        string USBPort { get; }
    }
}
