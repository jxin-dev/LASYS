using LASYS.BarcodeAnalyzer.Events;
using LASYS.BarcodeAnalyzer.Models;

namespace LASYS.BarcodeAnalyzer.Interfaces
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
