using LASYS.Application.Contracts;
using LASYS.Application.Events;
using LASYS.Application.Features.Devices.Events;
using LASYS.Application.Features.Devices.Models;
namespace LASYS.Application.Interfaces.Services
{
    public interface IBarcodeService
    {
        Task<BarcodeConfig?> LoadAsync();
        Task SaveAsync(BarcodeConfig config);

        IReadOnlyList<string> GetUSBVirtualCOMPortList();
        IReadOnlyList<string> GetManualCOMList(int start = 1, int end = 5);

        // Events
        event EventHandler<BarcodeNotificationEventArgs> BarcodeNotification;
        event EventHandler<BarcodeStatusEventArgs> BarcodeStatusChanged;

        public event EventHandler<DeviceStatusChangedEventArgs>? DeviceStatusChanged;
        DeviceStatus CurrentStatus { get; }

        event EventHandler<BarcodeScannedEventArgs> BarcodeScanned;
        bool IsConnected { get; }
        Task InitializeAsync();
        Task ScanAsync();
        Task SetManualModeAsync();

    }
}
