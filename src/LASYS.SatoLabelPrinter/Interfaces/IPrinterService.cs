using LASYS.SatoLabelPrinter.Events;
using LASYS.SatoLabelPrinter.Models;

namespace LASYS.SatoLabelPrinter.Interfaces
{
    public interface IPrinterService : IDisposable
    {
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
        void LoadLabelTemplate(string templatePath);
        void SetLabelVariables(Dictionary<string, string> data);
        bool PrintLabelWithPreview(string filename, int previewWidth = 800, int previewHeight = 600);
        void Print(int timeoutSeconds = 300);

        event EventHandler<LabelPrintEventArgs> LabelPrintProgress;
        event EventHandler<PrinterStateChangedEventArgs> PrinterStateChanged;
        event EventHandler<PrinterNotificationEventArgs> PrinterNotification;

    }
}
