using LASYS.Application.Features.Devices.Enums;
using LASYS.Application.Features.Devices.Events;
using LASYS.Application.Features.Devices.Models;
using LASYS.Application.Interfaces.Services.Camera;

namespace LASYS.Application.Interfaces.Services
{
    public interface IDeviceManager
    {
        IPrinterService Printer { get; }
        ICameraService Camera { get; }
        IBarcodeService Barcode { get; }
        Task InitializeAllAsync();
        Task ShutdownAllAsync();
        DeviceStatus? GetLastStatus(DeviceType device);
        bool IsDeviceConnected(DeviceType type);
        bool IsAnyDeviceDisconnected();

        event EventHandler<DeviceStatusChangedEventArgs> DeviceStatusChanged;
    }
}
