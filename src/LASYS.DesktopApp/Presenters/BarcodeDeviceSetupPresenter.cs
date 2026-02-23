using LASYS.BarcodeAnalyzer.Events;
using LASYS.BarcodeAnalyzer.Interfaces;
using LASYS.DesktopApp.Views.Interfaces;

namespace LASYS.DesktopApp.Presenters
{
    public class BarcodeDeviceSetupPresenter
    {
        public UserControl View { get; }
        private readonly IBarcodeDeviceSetupView _view;
        private readonly IBarcodeService _barcodeService;

        public BarcodeDeviceSetupPresenter(IBarcodeDeviceSetupView view, IBarcodeService barcodeService)
        {
            _view = view;
            View = (UserControl)view;

            _barcodeService = barcodeService;

            _view.LoadRequested += OnLoadRequested;

            _barcodeService.BarcodeStatusChanged += OnBarcodeStatusChanged;

        }

        private void OnBarcodeStatusChanged(object? sender, BarcodeStatusEventArgs e)
        {
            _view.InvokeOnUI(()=> _view.DisplayBarcodeStatus(e.Message, e.IsError));
        }

        private async void OnLoadRequested(object? sender, EventArgs e)
        {
            var config = await _barcodeService.LoadAsync();
            if (config?.Port != null)
            {
                var usbPorts = _barcodeService.GetUSBVirtualCOMPortList();
                _view.SetUSBVirtualCOMPortList(usbPorts);
                _view.SetSelectedPort(config.Port);
            }
            else
            {
                OnBarcodeStatusChanged(this, new BarcodeStatusEventArgs("No barcode device configured.", true));
            }
        }
    }
}
