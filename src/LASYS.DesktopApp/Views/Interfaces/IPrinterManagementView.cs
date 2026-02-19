namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface IPrinterManagementView
    {
        string SelectedInterfaceType { get; }
        string ComPort { get; }
        string UsbId { get; }

        event EventHandler ConnectionTypeChanged;
        void SetPort(int comboWidth, string portTitle);
        void SetPortList(IReadOnlyList<string> ports);
        void SetSelectedPort(string printerInterface, string port);

        event EventHandler SaveClicked;
        event EventHandler TestPrintClicked;
        event EventHandler LoadRequested;


        void ShowMessage(string message, string caption, MessageBoxIcon icon);
        void ReportPrinterState(string message, bool isError = false);
        void UpdateTestPrintButtonState(bool isEnabled);
        void InvokeOnUI(Action action);
    }
}
