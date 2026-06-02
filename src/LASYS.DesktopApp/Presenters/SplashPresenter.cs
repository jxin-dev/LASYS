using LASYS.Application.Events;
using LASYS.Application.Features.Devices.Enums;
using LASYS.Application.Features.Devices.Events;
using LASYS.Application.Features.Devices.Models;
using LASYS.Application.Interfaces.Services;
using LASYS.DesktopApp.Views.Forms;
using LASYS.DesktopApp.Views.Interfaces;

namespace LASYS.DesktopApp.Presenters
{
    public class SplashPresenter
    {
        private ISplashView _view;
        private readonly IDeviceManager _deviceManager;
        public SplashForm View { get; }
        private readonly Dictionary<DeviceType, bool> _lastConnectionState = new();
        public SplashPresenter(ISplashView view, IDeviceManager deviceManager)
        {
            _view = view;
            _deviceManager = deviceManager;
            View = (SplashForm)view;

            _deviceManager.DeviceStatusChanged += OnDeviceStatusChanged;
            _view.ViewShown += OnViewShown;
        }

        private void OnDeviceStatusChanged(object? sender, DeviceStatusChangedEventArgs e)
        {
            var status = e.Status;
            
            HandleConnectionStateChange(status);
        }
        private void HandleConnectionStateChange(DeviceStatus status)
        {
            var device = status.Device;
            var isConnected = status.IsConnected;

            if (_lastConnectionState.TryGetValue(device, out var previous))
            {
                // no change → ignore
                if (previous == isConnected)
                    return;
            }

            _lastConnectionState[device] = isConnected;

            if (!isConnected)
            {
                _view.InvokeOnUI(() => _view.UpdateProgress(GetProgress(status.Device), $"❌ {device} disconnected"));
            }
            else
            {
                _view.InvokeOnUI(() => _view.UpdateProgress(GetProgress(status.Device), $"✅ {device} connected"));
            }

        }

        private int GetProgress(DeviceType device) => device switch
        {
            DeviceType.Camera => 20,
            DeviceType.BarcodeScanner => 40,
            DeviceType.Printer => 70,
            _ => 0
        };



        private async void OnViewShown(object? sender, EventArgs e)
        {
            await InitializeAsync();

        }
        public async Task InitializeAsync()
        {
            _view.InvokeOnUI(() => _view.UpdateProgress(0, "Loading, Please wait..."));
            await _deviceManager.InitializeAllAsync();
            _view.InvokeOnUI(() => _view.UpdateProgress(90, "Finalizing setup..."));
            await Task.Delay(2000);
            _view.InvokeOnUI(() => _view.UpdateProgress(100, "Launching application..."));
            await Task.Delay(800);

            _view.CloseView();

        }
    }
}
