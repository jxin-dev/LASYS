using LASYS.Application.Contracts;
using LASYS.Application.Events;
using LASYS.Application.Features.Devices.Events;
using LASYS.Application.Features.Devices.Models;

namespace LASYS.Application.Interfaces.Services
{
    public interface IPrinterService : IDisposable
    {
        DeviceStatus CurrentStatus { get; }
        string? PrinterName { get; }
        //Comfiguration management
        Task<PrinterConfig?> LoadAsync();
        Task SaveAsync(PrinterConfig config);

        //Other printer related operations can be added here in the future
        Task InitializeAsync();
        IReadOnlyList<string> GetUSBList();
        IReadOnlyList<string> GetCOMList();
        IReadOnlyList<string> GetManualCOMList(int start = 1, int end = 5);
        void TestPrint();

        //Label template management
        //void LoadLabelTemplate(string templatePath);
        //void SetLabelVariables(Dictionary<string, string> data);
        //bool PrintLabelWithPreview(string filename, int previewWidth = 800, int previewHeight = 600);
        Task<bool> IsPrinted(string prnFilePath, int timeoutSeconds = 300);

        event EventHandler<LabelEventArgs> LabelStatusChanged;
        //event EventHandler<PrinterStatusEventArgs> PrinterStatusChanged;
        event EventHandler<DeviceStatusChangedEventArgs> DeviceStatusChanged;
        event EventHandler<PrinterNotificationEventArgs> PrinterNotification;

    }
}
