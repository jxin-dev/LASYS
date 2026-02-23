using LASYS.DesktopApp.Views.Interfaces;

namespace LASYS.DesktopApp.Views.UserControls
{
    public partial class BarcodeDeviceSetupControl : UserControl, IBarcodeDeviceSetupView
    {
        public event EventHandler? LoadRequested;
        public BarcodeDeviceSetupControl()
        {
            InitializeComponent();
            Load += delegate { LoadRequested?.Invoke(this, EventArgs.Empty); };
        }


        public void DisplayBarcodeStatus(string message, bool isError = false)
        {
            lblBarcodeStatus.ForeColor = isError ? Color.Red : Color.Green;
            lblBarcodeStatus.Text = message;
        }

        public void InvokeOnUI(Action action)
        {
            if (this.InvokeRequired)
                this.Invoke(action);
            else
                action();
        }

        public void SetSelectedPort(string port)
        {
            cbxPort.SelectedIndex = cbxPort.FindStringExact(port);
        }

        public void SetUSBVirtualCOMPortList(IReadOnlyList<string> ports)
        {
            cbxPort.Items.Clear();

            if (ports != null)
            {
                var portList = ports.ToList();
                if (portList.Any())
                {
                    cbxPort.Items.AddRange(portList.ToArray());
                    cbxPort.SelectedIndex = 0; // optional
                }
            }
            // Limit drop-down height
            cbxPort.MaxDropDownItems = 5; // max visible items
            cbxPort.DropDownHeight = cbxPort.ItemHeight * cbxPort.MaxDropDownItems;
        }
    }
}
