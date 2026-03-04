using LASYS.Application.Contracts;
using LASYS.Application.Events;
namespace LASYS.Application.Interfaces
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
        event EventHandler<BarcodeScannedEventArgs> BarcodeScanned;

        Task InitializeAsync();
        Task ScanAsync();
        Task SetManualModeAsync();

    }
}
