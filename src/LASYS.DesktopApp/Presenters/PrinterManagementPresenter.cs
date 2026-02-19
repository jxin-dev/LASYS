using System.Windows.Forms;
using LASYS.DesktopApp.Views.Interfaces;
using LASYS.SatoLabelPrinter.Events;
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

            _printerService.PrinterNotification += OnPrinterNotification;
            _printerService.PrinterStateChanged += OnPrinterStateChanged;
        }

        private void OnPrinterStateChanged(object? sender, PrinterStateChangedEventArgs e)
        {
            _view.InvokeOnUI(() => _view.ReportPrinterState(e.Message, e.Status == PrinterStatus.Error));
        }

        private void OnPrinterNotification(object? sender, PrinterNotificationEventArgs e)
        {
            var messageBoxIcon = e.MessageType switch
            {
                PrinterMessageType.Info => MessageBoxIcon.Information,
                PrinterMessageType.Error => MessageBoxIcon.Error,
                _ => MessageBoxIcon.Information
            };

            _view.ShowMessage(e.Message, "Printer Setup", messageBoxIcon);
        }

        private async void OnSaveClicked(object? sender, EventArgs e)
        {
            try
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
                        UsbId = !string.IsNullOrWhiteSpace(_view.UsbId) ? _view.UsbId : throw new InvalidOperationException("USB ID cannot be null or empty.")
                    },
                    _ => throw new InvalidOperationException("Invalid interface type")
                };

                var config = new PrinterConfig
                {
                    SatoPrinter = connection
                };
                await _printerService.SaveAsync(config);
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "Printer Setup", MessageBoxIcon.Error);
            }
           
        }

        private void OnConnectionTypeChanged(object? sender, EventArgs e)
        {
            _view.ReportPrinterState("");
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
                _view.SetPort(600, "Select USB id");
            }
        }
    }
}
