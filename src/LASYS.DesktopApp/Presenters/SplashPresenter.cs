using LASYS.Application.Common.Enums;
using LASYS.Application.Events;
using LASYS.Application.Features.LabelProcessing.Abstractions;
using LASYS.DesktopApp.Views.Forms;
using LASYS.DesktopApp.Views.Interfaces;

namespace LASYS.DesktopApp.Presenters
{
    public class SplashPresenter
    {
        private ISplashView _view;
        public SplashForm View { get; }
        private readonly ILabelProcessingService _labelProcessingService;
        public SplashPresenter(ISplashView view, ILabelProcessingService labelProcessingService)
        {
            _view = view;
            View = (SplashForm)view;

            _view.ViewShown += OnViewShown;
            _labelProcessingService = labelProcessingService;
            _labelProcessingService.DeviceStatusChanged += OnDeviceStatusChanged;
        }

        private void OnDeviceStatusChanged(object? sender, DeviceStatusEventArgs e)
        {
            switch (e.Device)
            {
                case DeviceType.Camera:
                    _view.InvokeOnUI(() => _view.UpdateProgress(15, e.Description));
                    break;
                case DeviceType.Printer:
                    _view.InvokeOnUI(() => _view.UpdateProgress(50, e.Description));
                    break;
                case DeviceType.Barcode:
                    _view.InvokeOnUI(() => _view.UpdateProgress(70, e.Description));
                    break;
                default:
                    break;
            }
        }

        private void OnBarcodeStatusChanged(object? sender, BarcodeStatusEventArgs e)
        {
            _view.InvokeOnUI(() => _view.UpdateProgress(70, e.Message));
        }
        private void OnPrinterStatusChanged(object? sender, PrinterStatusEventArgs e)
        {
            _view.InvokeOnUI(() => _view.UpdateProgress(50, e.Message));
        }

        private async void OnViewShown(object? sender, EventArgs e)
        {
            await InitializeAsync();

        }
        private void OnCameraConfigIssue(object? sender, CameraConfigEventArgs e)
        {
            if (_view == null) return;
            // Ensure UI thread update if needed
            _view.InvokeOnUI(() => _view.UpdateProgress(15, e.Message));
        }

        public async Task InitializeAsync()
        {
            _view.InvokeOnUI(() => _view.UpdateProgress(0, "Loading, Please wait..."));
            await Task.Delay(500);

            _view.InvokeOnUI(() => _view.UpdateProgress(10, "Initializing camera, printer, and barcode scanner..."));
            await _labelProcessingService.InitializeDevicesAsync();
            await Task.Delay(500);

            _view.InvokeOnUI(() => _view.UpdateProgress(96, "Finalizing setup..."));
            await Task.Delay(2000);
            _view.InvokeOnUI(() => _view.UpdateProgress(100, "Launching application..."));
            await Task.Delay(1000);

            _view.CloseView();

        }
    }
}
