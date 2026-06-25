using System.Drawing;
using LASYS.Application.Features.Devices.Enums;
using LASYS.Application.Features.Devices.Events;
using LASYS.Application.Features.Devices.Models;
using LASYS.Application.Interfaces.Services;
using LASYS.Application.Interfaces.Services.Camera;
using LASYS.Infrastructure.Hardware.Camera;

namespace LASYS.Infrastructure.Hardware.DeviceManagement
{
    public sealed class DeviceManager : IDeviceManager
    {
        public IPrinterService Printer { get; }
        public ICameraService Camera { get; }
        public IBarcodeService Barcode { get; }

        public event EventHandler<DeviceStatusChangedEventArgs>? DeviceStatusChanged;
        private readonly Dictionary<DeviceType, DeviceStatus> _latestStatus = new();
        public DeviceManager(IPrinterService printer, ICameraService camera, IBarcodeService barcode)
        {
            Printer = printer;
            Camera = camera;
            Barcode = barcode;

            Printer.DeviceStatusChanged += OnDeviceStatusChanged;
            Camera.DeviceStatusChanged += OnDeviceStatusChanged;
            Barcode.DeviceStatusChanged += OnDeviceStatusChanged;

        }

        private void OnDeviceStatusChanged(object? sender, DeviceStatusChangedEventArgs e)
        {
            _latestStatus[e.Status.Device] = e.Status;
            DeviceStatusChanged?.Invoke(this, e);
        }

        public async Task InitializeAllAsync()
        {
            try
            {
                await Camera.InitializeAsync();
                await Barcode.InitializeAsync();
                await Printer.InitializeAsync();
            }
            catch (Exception)
            {

            }
          
        }

        public async Task ShutdownAllAsync()
        {
            await Camera.StopAsync();
            // printer usually has no async stop in your design
            Printer.Dispose();
        }

        public bool IsDeviceConnected(DeviceType type)
        {
            return type switch
            {
                DeviceType.Camera => Camera.CurrentStatus.IsConnected,
                DeviceType.Printer => Printer.CurrentStatus.IsConnected,
                DeviceType.BarcodeScanner => Barcode.CurrentStatus.IsConnected,
                _ => false
            };
        }

        public bool IsAnyDeviceDisconnected()
        {
            return !Camera.CurrentStatus.IsConnected
                || !Printer.CurrentStatus.IsConnected
                || !Barcode.CurrentStatus.IsConnected;
        }

        public DeviceStatus? GetLastStatus(DeviceType device)
        {
            return _latestStatus.TryGetValue(device, out var status) ? status : null;
        }
    }
}
