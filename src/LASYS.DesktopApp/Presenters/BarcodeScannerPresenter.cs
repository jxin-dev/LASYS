using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Messaging;
using LASYS.Application.Contracts;
using LASYS.Application.Events;
using LASYS.Application.Interfaces;
using LASYS.DesktopApp.Views.Interfaces;

namespace LASYS.DesktopApp.Presenters
{
    public class BarcodeScannerPresenter
    {
        public UserControl View { get; }
        private readonly IBarcodeScannerView _view;
        private readonly IBarcodeService _barcodeService;

        public BarcodeScannerPresenter(IBarcodeScannerView view, IBarcodeService barcodeService)
        {
            _view = view;
            View = (UserControl)view;

            _barcodeService = barcodeService;

            _view.LoadRequested += OnLoadRequested;
            _view.SaveClicked += OnSaveClicked;
            _view.SetManualModeClicked += OnSetManualModeClicked;

            _barcodeService.BarcodeStatusChanged += OnBarcodeStatusChanged;
            _barcodeService.BarcodeNotification += OnBarcodeNotification;

        }

        private async void OnSetManualModeClicked(object? sender, EventArgs e)
        {
            await _barcodeService.SetManualModeAsync();
        }

        private void OnBarcodeNotification(object? sender, BarcodeNotificationEventArgs e)
        {
            var messageBoxIcon = e.MessageType switch
            {
                MessageType.Info => MessageBoxIcon.Information,
                MessageType.Error => MessageBoxIcon.Error,
                _ => MessageBoxIcon.Information
            };
            _view.ShowNotification(e.Message, "Barcode Device", messageBoxIcon);
        }

        private async void OnSaveClicked(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_view.USBPort))
            {
                OnBarcodeStatusChanged(this, new BarcodeStatusEventArgs(BarcodeStatus.BarcodeNotConfigured));
                return;
            }

            var config = new BarcodeConfig
            {
                Port = _view.USBPort
            };
            await _barcodeService.SaveAsync(config);
        }

        private void OnBarcodeStatusChanged(object? sender, BarcodeStatusEventArgs e)
        {
            switch (e.Status)
            {
                case BarcodeStatus.BarcodeCommunicating:
                case BarcodeStatus.BarcodeConnected:
                case BarcodeStatus.BarcodeReconnecting:
                case BarcodeStatus.BarcodeScanning:
                case BarcodeStatus.BarcodeScanned:
                    _view.InvokeOnUI(() => _view.DisplayBarcodeStatus(e.Description, false));
                    break;
                case BarcodeStatus.BarcodeNotConfigured:
                case BarcodeStatus.BarcodeNotDetected:
                case BarcodeStatus.BarcodeDisconnected:
                case BarcodeStatus.BarcodeTimeout:
                case BarcodeStatus.BarcodeError:
                    _view.InvokeOnUI(() => _view.DisplayBarcodeStatus(e.Description, true));
                    break;
                default:
                    break;
            }
           
        }

        private async void OnLoadRequested(object? sender, EventArgs e)
        {
            var config = await _barcodeService.LoadAsync();
            if (config?.Port != null)
            {
                //var usbPorts = _barcodeService.GetUSBVirtualCOMPortList();
                var usbPorts = _barcodeService.GetManualCOMList();
                _view.SetUSBVirtualCOMPortList(usbPorts);
                _view.SetSelectedPort(config.Port);
            }
            else
            {
                //var usbPorts = _barcodeService.GetUSBVirtualCOMPortList();
                var usbPorts = _barcodeService.GetManualCOMList();
                _view.SetUSBVirtualCOMPortList(usbPorts);
                OnBarcodeStatusChanged(this, new BarcodeStatusEventArgs(BarcodeStatus.BarcodeNotConfigured));
            }
        }
    }
}
