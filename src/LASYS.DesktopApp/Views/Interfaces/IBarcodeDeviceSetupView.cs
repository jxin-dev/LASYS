namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface IBarcodeDeviceSetupView
    {
        void InvokeOnUI(Action action);

        event EventHandler LoadRequested;
        void DisplayBarcodeStatus(string message, bool isError = false);
        void SetUSBVirtualCOMPortList(IReadOnlyList<string> ports);
        void SetSelectedPort(string port);
    }
}
