using LASYS.BarcodeAnalyzer.Events;
using LASYS.BarcodeAnalyzer.Models;

namespace LASYS.BarcodeAnalyzer.Interfaces
{
    public interface IBarcodeService
    {
        Task<BarcodeConfig?> LoadAsync();
        Task SaveAsync(BarcodeConfig config);

        // Events
        event EventHandler<BarcodeNotificationEventArgs> BarcodeNotification;
        event EventHandler<BarcodeStatusEventArgs> BarcodeStatusChanged;
        event EventHandler<BarcodeScannedEventArgs> BarcodeScanned;

        Task InitializeAsync();
        Task ScanAsync();

    }
}
