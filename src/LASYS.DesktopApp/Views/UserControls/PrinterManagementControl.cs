using LASYS.DesktopApp.Views.Interfaces;
using LASYS.SatoLabelPrinter.Interfaces;

namespace LASYS.DesktopApp.Views.UserControls
{
    public partial class PrinterManagementControl : UserControl, IPrinterManagementView
    {

        public string SelectedInterfaceType => cbxInterface.SelectedItem?.ToString() ?? string.Empty;
        public string ComPort => cbxPort.SelectedItem?.ToString() ?? string.Empty;
        public string UsbId => cbxPort.SelectedItem?.ToString() ?? string.Empty;

        public event EventHandler? ConnectionTypeChanged;
        public event EventHandler? SaveClicked;
        public event EventHandler? TestPrintClicked;
        public event EventHandler? LoadRequested;

        public PrinterManagementControl()
        {
            InitializeComponent();
            cbxInterface.SelectedIndexChanged += (sender, e) => ConnectionTypeChanged?.Invoke(this, EventArgs.Empty);
            btnSavePrinter.Click += (sender, e) => SaveClicked?.Invoke(this, EventArgs.Empty);
            btnTestPrint.Click += (sender, e) => TestPrintClicked?.Invoke(this, EventArgs.Empty);
            btnTestPrint.Visible = false;

            Load += delegate { LoadRequested?.Invoke(this, EventArgs.Empty); };
        }


        public void SetPort(int comboWidth, string portTitle)
        {
            lblPort.Text = portTitle;
            cbxPort.Width = comboWidth;
        }

        public void SetPortList(IReadOnlyList<string> ports)
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
            cbxPort.MaxDropDownItems = 10; // max visible items
            cbxPort.DropDownHeight = cbxPort.ItemHeight * cbxPort.MaxDropDownItems;
        }

        public void ShowMessage(string message, string caption, MessageBoxIcon icon)
        {
            MessageBox.Show(message, caption, MessageBoxButtons.OK, icon);
        }

        public void ReportPrinterState(string message, bool isError = false)
        {
            lblPrinterState.ForeColor = isError ? Color.Red : Color.Green;
            lblPrinterState.Text = message;
        }

        public void InvokeOnUI(Action action)
        {
            if (this.InvokeRequired)
                this.Invoke(action);
            else
                action();
        }

        public void UpdateTestPrintButtonState(bool isEnabled)
        {
            btnTestPrint.Visible = isEnabled;
        }

        public void SetSelectedPort(string printerInterface, string port)
        {
            cbxInterface.SelectedIndex = cbxInterface.FindStringExact(printerInterface);
            cbxPort.SelectedIndex = cbxPort.FindStringExact(port);
        }
    }
}
