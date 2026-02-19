using Interop.LabelGalleryPlus3WR;
using SATOPrinterAPI;
using System.Runtime.InteropServices;
using System.Text;
using static SATOPrinterAPI.Printer;

namespace LASYS.Infrastructure.Printing;

public class SatoLabelPrinter : IDisposable
{
    private LGApp? _app;
    private LGLabel? _label;
    private Printer? _printer;
    private bool _disposed;

    public PrinterSettings Settings { get; } = new();

    public string PRNPath { get; private set; } = string.Empty;
    public string IMGPath { get; private set; } = string.Empty;

    public SatoLabelPrinter(string templatePath)
    {
        if (!File.Exists(templatePath))
            throw new FileNotFoundException("Label template not found", templatePath);

        _app = new LGApp();
        _label = _app.LabelOpenEx(templatePath);
    }

    public void FillTemplate(Dictionary<string, string> data)
    {
        if (_label == null) return;

        foreach (var (key, value) in data)
        {
            var variable = _label.Variables.FindByName(key);
            variable?.SetValue(value);
        }
    }

    public bool Process(string filename, int previewWidth = 800, int previewHeight = 600)
    {
        try
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string dirPrn = Path.Combine(baseDir, "prns");
            string dirImage = Path.Combine(baseDir, "images");

            Directory.CreateDirectory(dirPrn);
            Directory.CreateDirectory(dirImage);

            PRNPath = Path.Combine(dirPrn, $"{filename}.prn");
            IMGPath = Path.Combine(dirImage, $"{filename}.png");

            _label!.PrinterName = Settings.PrinterName;
            _label.PrinterPort = dirPrn; // LabelGallery uses this as the output directory for file printing

            return RunFuncWithRetry(() =>
            {
                // Print("1") generates the PRN file in the PrinterPort directory
                bool printed = _label.Print("1");
                bool previewed = _label.GetLabelPreview(IMGPath, previewWidth, previewHeight);
                return printed && previewed;
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Processing Error: {ex.Message}");
            return false;
        }
    }

    public bool Print(int timeoutSeconds = 300)
    {
        if (!File.Exists(PRNPath)) return false;

        using var eventCompleted = new ManualResetEvent(false);

        // Define handler
        EventHandler<ByteAvailableEventArgs> handler = (s, e) =>
        {
            string message = Encoding.ASCII.GetString(e.Data);
            Console.WriteLine($"📨 Printer Response: {message}");
            eventCompleted.Set();
        };

        try
        {
            if (!Configure(handler)) return false;

            byte[] prnData = File.ReadAllBytes(PRNPath);
            _printer!.Send(prnData);

            Console.WriteLine($"✅ PRN file sent to printer. Waiting for response...");

            if (!eventCompleted.WaitOne(TimeSpan.FromSeconds(timeoutSeconds)))
            {
                Console.WriteLine("⚠️ Timed out waiting for printer response.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Print Error: {ex.Message}");
            return false;
        }
        finally
        {
            if (_printer != null) _printer.ByteAvailable -= handler; // Always unsubscribe!
        }
    }

    private bool Configure(EventHandler<ByteAvailableEventArgs> handler)
    {
        _printer ??= new Printer();
        _printer.Interface = Settings.Interface;

        switch (Settings.Interface)
        {
            case InterfaceType.USB:
                if (string.IsNullOrEmpty(Settings.PortIDOrIP))
                {
                    var usbPorts = _printer.GetUSBList();
                    if (usbPorts?.Count > 0)
                        Settings.PortIDOrIP = usbPorts[0].PortID;
                    else
                        throw new Exception("No USB SATO printer found.");
                }
                _printer.USBPortID = Settings.PortIDOrIP;
                break;

            case InterfaceType.TCPIP:
                _printer.TCPIPPort = Settings.PortIDOrIP ?? throw new ArgumentException("IP required for TCP");
                break;

            default:
                throw new NotSupportedException($"Interface {Settings.Interface} not configured.");
        }

        _printer.ByteAvailable += handler;
        return true;
    }

    private bool RunFuncWithRetry(Func<bool> action, int maxRetry = 3, int delayMs = 500)
    {
        for (int i = 0; i < maxRetry; i++)
        {
            if (action()) return true;
            if (i < maxRetry - 1) Thread.Sleep(delayMs);
        }
        return false;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            // Dispose managed state
            try { _printer?.Disconnect(); } catch { }
        }

        // Release COM objects - CRITICAL for LabelGallery
        if (_label != null)
        {
            try { _label.Free(); } catch { }
            if (OperatingSystem.IsWindows()) Marshal.ReleaseComObject(_label);
            _label = null;
        }

        if (_app != null)
        {
            try
            {
                _app.Free();
                _app.Quit();
            }
            catch { }
            if (OperatingSystem.IsWindows()) Marshal.ReleaseComObject(_app);
            _app = null;
        }

        _disposed = true;
    }

    ~SatoLabelPrinter() => Dispose(false);
}