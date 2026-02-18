using LASYS.DesktopApp.Views.Interfaces;
using LASYS.SatoLabelPrinter.Interfaces;
using LASYS.SatoLabelPrinter.Models;

namespace LASYS.DesktopApp.Presenters
{
    public class PrinterManagementPresenter
    {
        public UserControl View { get; }
        private readonly IPrinterManagementView _view;
        private readonly IPrinterService _printerService;

        public PrinterManagementPresenter(IPrinterManagementView view, IPrinterService printerService)
        {
            _view = view;
            _printerService = printerService;
            View = (UserControl)view;

            _view.ConnectionTypeChanged += OnConnectionTypeChanged;
            _view.SaveClicked += OnSaveClicked;
        }

        private async void OnSaveClicked(object? sender, EventArgs e)
        {
            IPrinterConnection connection = _view.SelectedInterfaceType switch
            {
                "Serial COM" => new SerialPrinterConnection
                {
                    ComPort = _view.ComPort,
                    BaudRate = 9600,
                    Parameters = "8N1"
                },
                "USB Port" => new UsbPrinterConnection
                {
                    UsbId = _view.UsbId
                },
                _ => throw new InvalidOperationException("Invalid interface type")
            };

            var config = new PrinterConfig
            {
                SatoPrinter = connection
            };
            await _printerService.SaveAsync(config);
        }

        private void OnConnectionTypeChanged(object? sender, EventArgs e)
        {
            if (_view.SelectedInterfaceType == "Serial COM")
            {
                //var comPorts = _printerService.GetCOMList();
                var comPorts = _printerService.GetManualCOMList(1, 50);
                _view.SetPortList(comPorts);
                _view.SetPort(180, "Select COM port");
            }
            else if (_view.SelectedInterfaceType == "USB Port")
            {
                var usbPorts = _printerService.GetUSBList();
                _view.SetPortList(usbPorts);
                _view.SetPort(600, "Select USB port");
            }
        }
    }
}
