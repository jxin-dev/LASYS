using System.IO.Ports;
using System.Management;
using System.Runtime.InteropServices;
using System.Text.Json;
using Interop.LabelGalleryPlus3WR;
using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Extensions;
using LASYS.Application.Common.Messaging;
using LASYS.Application.Contracts;
using LASYS.Application.Events;
using LASYS.Application.Interfaces.Services;
using LASYS.Application.Models.Hardware.Printer;
using SATOPrinterAPI;

namespace LASYS.Infrastructure.Hardware.Printers.Sato
{
    public class PrinterService : IPrinterService
    {
        private readonly string _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "printer.config.json");
        private readonly ManualResetEventSlim _printWaitHandle = new(false);

        private LGApp? _app;
        private LGLabel? _label;
        private Printer? _printer;
        private bool _disposed;
        private string? _printerName;
        private string _prnFilePath = string.Empty;
        private bool _isByteAvailableSubscribed;

        public event EventHandler<PrinterNotificationEventArgs>? PrinterNotification;

        public event EventHandler<PrinterStatusEventArgs>? PrinterStatusChanged;
        public event EventHandler<LabelEventArgs>? LabelStatusChanged;

        public string FilePath => _prnFilePath;

        private void EnsureByteAvailableSubscription()
        {
            if (_printer == null || _isByteAvailableSubscribed)
                return;

            _printer.ByteAvailable += AsynDataIn;
            _isByteAvailableSubscribed = true;
        }
        private readonly Dictionary<char, PrinterStatus> _printerControlStatusMap = new()
        {
            { '\x11', PrinterStatus.PrintStarted },  // DC1
            { '\x13', PrinterStatus.PrinterPaused },    // DC3
            // You can add more single-char mappings here
        };
        private void AppendSendText(byte[] data)
        {
            string smsg = ControlCharConvert(Utils.ByteArrayToString(data));

            PrinterStatusChanged?.Invoke(this,
                new PrinterStatusEventArgs(
                    PrinterStatus.PrinterDataSent,
                    $"{DateTime.Now:HH:mm:ss.fff} | TX {data.Length} bytes | {smsg}"
                ));
        }
        private void AsynDataIn(object? sender, Printer.ByteAvailableEventArgs e)
        {
            AppendRecvText(e.Data, false);

            foreach (char ch in e.Data)
            {
                if (_printerControlStatusMap.TryGetValue(ch, out var status))
                {
                    // Raise event with default message
                    PrinterStatusChanged?.Invoke(this, new PrinterStatusEventArgs(status));
                }
            }
            // Check multi-byte sequence: ESC+@ (0x1B, 0x40) for Offline
            for (int i = 0; i < e.Data.Length - 1; i++)
            {
                if (e.Data[i] == 0x1B && e.Data[i + 1] == 0x40)
                {
                    PrinterStatusChanged?.Invoke(this, new PrinterStatusEventArgs(PrinterStatus.PrinterOffline));
                }
            }

            _printWaitHandle.Set();
        }
        private void AppendRecvText(byte[] data, bool empty)
        {
            // Convert bytes to string
            string smsg = ControlCharConvert(Utils.ByteArrayToString(data));
            PrinterStatusChanged?.Invoke(this, new PrinterStatusEventArgs(PrinterStatus.PrinterDataReceived, $"{DateTime.Now:HH:mm:ss.fff} | RX {data.Length} bytes | {smsg}"));
        }

        private static readonly Dictionary<char, string> _controlCharMap = ControlCharList().ToDictionary(x => x.Value, x => x.Key);

        private string ControlCharConvert(string data)
        {
            foreach (var kvp in _controlCharMap)
            {
                data = data.Replace(kvp.Key.ToString(), kvp.Value);
            }
            return data;
        }
        private static Dictionary<string, char> ControlCharList()
        {
            Dictionary<string, char> ctr = new Dictionary<string, char>();
            ctr.Add("[NUL]", '\u0000');
            ctr.Add("[SOH]", '\u0001');
            ctr.Add("[STX]", '\u0002');
            ctr.Add("[ETX]", '\u0003');
            ctr.Add("[EOT]", '\u0004');
            ctr.Add("[ENQ]", '\u0005');
            ctr.Add("[ACK]", '\u0006');
            ctr.Add("[BEL]", '\u0007');
            ctr.Add("[BS]", '\u0008');
            ctr.Add("[HT]", '\u0009');
            ctr.Add("[LF]", '\u000A');
            ctr.Add("[VT]", '\u000B');
            ctr.Add("[FF]", '\u000C');
            ctr.Add("[CR]", '\u000D');
            ctr.Add("[SO]", '\u000E');
            ctr.Add("[SI]", '\u000F');
            ctr.Add("[DLE]", '\u0010');
            ctr.Add("[DC1]", '\u0011');
            ctr.Add("[DC2]", '\u0012');
            ctr.Add("[DC3]", '\u0013');
            ctr.Add("[DC4]", '\u0014');
            ctr.Add("[NAK]", '\u0015');
            ctr.Add("[SYN]", '\u0016');
            ctr.Add("[ETB]", '\u0017');
            ctr.Add("[CAN]", '\u0018');
            ctr.Add("[EM]", '\u0019');
            ctr.Add("[SUB]", '\u001A');
            ctr.Add("[ESC]", '\u001B');
            ctr.Add("[FS]", '\u001C');
            ctr.Add("[GS]", '\u001D');
            ctr.Add("[RS]", '\u001E');
            ctr.Add("[US]", '\u001F');
            ctr.Add("[DEL]", '\u007F');
            return ctr;
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
                try
                {
                    if (_printer != null && _isByteAvailableSubscribed)
                    {
                        _printer.Disconnect();
                        _printer.ByteAvailable -= AsynDataIn;
                        _isByteAvailableSubscribed = false;
                    }
                }
                catch { }
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

        ~PrinterService() => Dispose(false);

        public async Task<PrinterConfig?> LoadAsync()
        {
            try
            {
                if (!File.Exists(_configPath))
                    return null;

                var json = await File.ReadAllTextAsync(_configPath);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var config = JsonSerializer.Deserialize<PrinterConfig>(json, options);

                if (config?.SatoPrinter is SerialPrinterConnection serial)
                {
                    PrinterStatusChanged?.Invoke(this, new PrinterStatusEventArgs(PrinterStatus.PrinterConfigurationLoaded, $"Printer configuration loaded successfully.\nCommunication will use serial interface on port {serial.ComPort} with settings {serial.Parameters} at {serial.BaudRate} baud."));
                }
                else if (config?.SatoPrinter is UsbPrinterConnection usb)
                {
                    PrinterStatusChanged?.Invoke(this, new PrinterStatusEventArgs(PrinterStatus.PrinterConfigurationLoaded, $"Printer configuration loaded successfully.\nCommunication will use the USB interface with device ID {usb.UsbId.Ellipsis(18)}"));
                }
                await Task.Delay(3000);
                return config;

            }
            catch (Exception ex)
            {
                PrinterNotification?.Invoke(this, new PrinterNotificationEventArgs(MessageType.Error, $"Failed to load printer configuration: {ex.Message}"));
                await Task.Delay(1000);
                return null;
            }
        }
        public async Task SaveAsync(PrinterConfig config)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(config, options);

                await File.WriteAllTextAsync(_configPath, json);

                PrinterNotification?.Invoke(this, new PrinterNotificationEventArgs(MessageType.Info, "Printer configuration saved successfully."));

            }
            catch (Exception ex)
            {
                PrinterNotification?.Invoke(this, new PrinterNotificationEventArgs(MessageType.Error, $"Failed to save printer configuration: {ex.Message}"));
                return;
            }

            await InitializeAsync();
        }
        public async Task InitializeAsync()
        {
            var config = await LoadAsync();
            if (config == null)
            {
                PrinterStatusChanged?.Invoke(this, new PrinterStatusEventArgs(PrinterStatus.PrinterNotConfigured));
                return;
            }

            if (_printer == null)
            {
                _printer = new Printer();
                _printer.PermanentConnect = true;
            }
            EnsureByteAvailableSubscription();

            if (config.SatoPrinter is SerialPrinterConnection serial)
            {
                _printer.Interface = Printer.InterfaceType.COM;
                _printer.COMPort = serial.ComPort;
                _printer.COMSetting.Baudrate = serial.BaudRate;
                _printer.COMSetting.Parameters = serial.Parameters;

                _printerName = GetPrinterNameByComPort(_printer.COMPort);
                if (_printerName == null)
                {
                    PrinterStatusChanged?.Invoke(this, new PrinterStatusEventArgs(PrinterStatus.PrinterNotDetected, $"No printer detected on COM port {_printer.COMPort}."));
                    return;
                }

                try
                {
                    _printer.Connect();
                    PrinterStatusChanged?.Invoke(this, new PrinterStatusEventArgs(PrinterStatus.PrinterConnected, $"Serial COM printer connected: {_printerName} (Port: {serial.ComPort})"));
                }
                catch (IOException ex)
                {
                    PrinterNotification?.Invoke(this, new PrinterNotificationEventArgs(MessageType.Error, $"Failed to connect to COM port {serial.ComPort}: {ex.Message}"));
                    return;
                }
            }
            else if (config.SatoPrinter is UsbPrinterConnection usb)
            {
                _printer.Interface = Printer.InterfaceType.USB;
                _printer.USBPortID = usb.UsbId;

                var usbPrinter = _printer.GetUSBList().FirstOrDefault(u => u.PortID == usb.UsbId);
                if (usbPrinter != null)
                {
                    _printerName = usbPrinter.Name; // Get the friendly name
                    _printer.Connect();
                    PrinterStatusChanged?.Invoke(this, new PrinterStatusEventArgs(PrinterStatus.PrinterConnected, $"USB printer connected: {_printerName}"));
                }
                else
                {
                    PrinterStatusChanged?.Invoke(this, new PrinterStatusEventArgs(PrinterStatus.PrinterDisconnected, $"USB printer {usb.UsbId} not connected."));
                    return;
                }
            }


        }

        public string? GetPrinterNameByComPort(string comPort)
        {
            if (string.IsNullOrWhiteSpace(comPort) || !OperatingSystem.IsWindows())
                return null;
            try
            {
                // Query all serial ports
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_SerialPort");
                foreach (ManagementObject port in searcher.Get())
                {
                    string deviceId = port["DeviceID"]?.ToString() ?? string.Empty; // e.g., COM3
                    string name = port["Name"]?.ToString() ?? string.Empty;        // e.g., "SATO CL6NX on COM50"

                    if (!string.IsNullOrEmpty(deviceId) && deviceId.Equals(comPort, StringComparison.OrdinalIgnoreCase))
                    {
                        return name; // Found matching port
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error detecting printer: {ex.Message}");
            }

            return null; // Return null if not found or on non-Windows platforms
        }
        public IReadOnlyList<string> GetUSBList()
        {
            _printer ??= new Printer();
            _printer.Interface = Printer.InterfaceType.USB;
            var usbPorts = _printer.GetUSBList();
            if (usbPorts == null || usbPorts.Count == 0)
            {
                Console.WriteLine("No USB printers found.");
                return Array.Empty<string>();
            }
            return usbPorts.Select(p => p.PortID).ToList();
        }
        public IReadOnlyList<string> GetCOMList()
        {
            return SerialPort.GetPortNames()
                     .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                     .ToList()
                     .AsReadOnly();
        }
        public IReadOnlyList<string> GetManualCOMList(int start = 1, int end = 5)
        {
            if (start < 1 || end < start)
                throw new ArgumentException("Invalid COM port range.");

            var list = Enumerable.Range(start, end - start + 1)
                                 .Select(i => $"COM{i}")
                                 .ToList()
                                 .AsReadOnly();

            return list;
        }
        public void TestPrint()
        {
            if (_printer is null)
            {
                PrinterStatusChanged?.Invoke(this, new PrinterStatusEventArgs(PrinterStatus.PrinterNotConfigured, "Printer not initialized."));
                return;
            }
            try
            {
                _printer.TestPrint();
            }
            catch (Exception ex)
            {
                PrinterStatusChanged?.Invoke(this,
                    new PrinterStatusEventArgs(PrinterStatus.PrinterError,
                    ex.Message));
            }
        }
        public void LoadLabelTemplate(string templatePath) //First step of printing is to load the label template, then fill data and print
        {
            if (!File.Exists(templatePath))
            {
                LabelStatusChanged?.Invoke(this, new LabelEventArgs(LabelStatus.TemplateLoadFailed, $"Label template not found: {templatePath}"));
                return;
            }

            _app ??= new LGApp();
            _label = _app.LabelOpenEx(templatePath);
            LabelStatusChanged?.Invoke(this, new LabelEventArgs(LabelStatus.TemplateLoaded, $"Label template loaded: {Path.GetFileName(templatePath)}"));
        }
        public void SetLabelVariables(Dictionary<string, string> data)
        {
            if (_label == null) return;

            foreach (var (key, value) in data)
            {
                var variable = _label.Variables.FindByName(key);
                variable?.SetValue(value);
            }
        }
        public bool PrintLabelWithPreview(string filename, int previewWidth = 800, int previewHeight = 600)
        {
            bool RunFuncWithRetry(Func<bool> action, int maxRetry = 3, int delayMs = 500)
            {
                for (int i = 0; i < maxRetry; i++)
                {
                    if (action()) return true;
                    if (i < maxRetry - 1) Thread.Sleep(delayMs);
                }
                return false;
            }

            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string dirPrn = Path.Combine(baseDir, "prns");
                string dirImage = Path.Combine(baseDir, "images");

                Directory.CreateDirectory(dirPrn);
                Directory.CreateDirectory(dirImage);

                _prnFilePath = Path.Combine(dirPrn, $"{filename}.prn");
                var imgPath = Path.Combine(dirImage, $"{filename}.png");

                if (_label == null)
                {
                    LabelStatusChanged?.Invoke(this, new LabelEventArgs(LabelStatus.TemplateLoadFailed));
                    return false;
                }
                if (string.IsNullOrWhiteSpace(_printerName))
                {
                    PrinterStatusChanged?.Invoke(this, new PrinterStatusEventArgs(PrinterStatus.PrinterNotConfigured));
                    return false;
                }

                _label.PrinterName = _printerName;
                _label.PrinterPort = dirPrn;

                return RunFuncWithRetry(() =>
                {
                    // Print("1") generates the PRN file in the PrinterPort directory
                    bool printed = _label.Print("1");
                    bool previewed = _label.GetLabelPreview(imgPath, previewWidth, previewHeight);
                    return printed && previewed;
                });
            }
            catch (Exception ex)
            {
                PrinterStatusChanged?.Invoke(this, new PrinterStatusEventArgs(PrinterStatus.PrinterError, $"Error during print process: {ex.Message}"));
                return false;
            }
        }
        public void Print(int timeoutSeconds = 300)
        {
            if (!File.Exists(_prnFilePath))
            {
                PrinterStatusChanged?.Invoke(this, new PrinterStatusEventArgs(PrinterStatus.PrinterError, $"PRN file not found: {_prnFilePath}."));
                return;
            }

            if (_printer is null)
            {
                PrinterStatusChanged?.Invoke(this, new PrinterStatusEventArgs(PrinterStatus.PrinterNotConfigured, "Printer not initialized. Please initialize the printer before printing."));
                return;
            }

            try
            {
                byte[] prnData = File.ReadAllBytes(_prnFilePath);
                _printWaitHandle.Reset(); // reset before sending
                AppendSendText(prnData);   // TX log
                _printer.Send(prnData);

                PrinterStatusChanged?.Invoke(this, new PrinterStatusEventArgs(PrinterStatus.PrintStarted, $"PRN file sent to printer: {_printerName}. Waiting for response..."));

                bool signaled = _printWaitHandle.Wait(TimeSpan.FromSeconds(timeoutSeconds));

                if (!signaled)
                {
                    PrinterStatusChanged?.Invoke(this, new PrinterStatusEventArgs(PrinterStatus.PrintFailed, $"Printer did not respond within {timeoutSeconds} seconds."));
                    throw new TimeoutException("Printer did not respond in time.");
                }

                PrinterStatusChanged?.Invoke(this, new PrinterStatusEventArgs(PrinterStatus.PrintCompleted));

            }
            catch (Exception ex)
            {
                PrinterStatusChanged?.Invoke(this, new PrinterStatusEventArgs(PrinterStatus.PrinterError, $"Error during printing: {ex.Message}"));
                throw;
            }
        }

    }


}
