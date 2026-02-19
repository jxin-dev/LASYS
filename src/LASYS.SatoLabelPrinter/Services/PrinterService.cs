using System.IO.Ports;
using System.Management;
using System.Runtime.InteropServices;
using System.Text.Json;
using Interop.LabelGalleryPlus3WR;
using LASYS.SatoLabelPrinter.Events;
using LASYS.SatoLabelPrinter.Interfaces;
using LASYS.SatoLabelPrinter.Models;
using SATOPrinterAPI;

namespace LASYS.SatoLabelPrinter.Services
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

        public event EventHandler<LabelPrintEventArgs>? LabelPrintProgress;
        public event EventHandler<PrinterStatusEventArgs>? PrinterStatusChanged;


        #region Printer Data Receiving & Processing
        private void EnsureByteAvailableSubscription()
        {
            if (_printer == null || _isByteAvailableSubscribed)
                return;

            _printer.ByteAvailable += AsynDataIn;
            _isByteAvailableSubscribed = true;
        }
        private readonly Dictionary<char, PrinterStatus> _printerControlStatusMap = new()
        {
            { '\x11', PrinterStatus.Printing },  // DC1
            { '\x13', PrinterStatus.Paused },    // DC3
            // You can add more single-char mappings here
        };

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
                    PrinterStatusChanged?.Invoke(this, new PrinterStatusEventArgs(PrinterStatus.Offline));
                }
            }

            _printWaitHandle.Set();
        }
        private void AppendRecvText(byte[] data, bool empty)
        {
            // Convert bytes to string
            string smsg = ControlCharConvert(Utils.ByteArrayToString(data));
            LabelPrintProgress?.Invoke(this, new LabelPrintEventArgs($"{DateTime.Now:yyyy-MM-dd HH:mm:ss:fff} | Received {data.Length} bytes | {smsg}"));

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
        #endregion

        #region Cleanup resources
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
        #endregion

        #region Configuration Loading & Printer Initialization
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
                    Console.WriteLine($"Serial Port: {serial.ComPort}");
                }
                else if (config?.SatoPrinter is UsbPrinterConnection usb)
                {
                    Console.WriteLine($"USB ID: {usb.UsbId}");
                }

                return config;

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to load printer.config.json: {ex.Message}");
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
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to save printer.config.json: {ex.Message}");
                throw;
            }

            await InitializeAsync();
        }
        public async Task InitializeAsync()
        {
            var config = await LoadAsync();
            if (config == null)
            {
                Console.WriteLine("No printer configuration found. Please set up your printer.");
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
                    PrinterStatusChanged?.Invoke(this, new PrinterStatusEventArgs(PrinterStatus.Error, $"No printer found on COM port {_printer.COMPort}"));
                    return;
                }

                try
                {
                    _printer.Connect();
                }
                catch (IOException ex)
                {
                    PrinterStatusChanged?.Invoke(this, new PrinterStatusEventArgs(PrinterStatus.Error, $"Failed to connect to COM port {serial.ComPort}: {ex.Message}"));
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
                    PrinterStatusChanged?.Invoke(this, new PrinterStatusEventArgs(PrinterStatus.Ready, $"USB printer connected: {_printerName}"));
                }
                else
                {
                    PrinterStatusChanged?.Invoke(this, new PrinterStatusEventArgs(PrinterStatus.Error, $"USB printer {usb.UsbId} not connected."));
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
                Console.WriteLine("Printer not initialized. Please initialize the printer before testing.");
                return;
            }

            _printer.TestPrint();
        }
        #endregion

        #region Printing Label Process
        public void LoadLabelTemplate(string templatePath) //First step of printing is to load the label template, then fill data and print
        {
            if (!File.Exists(templatePath))
            {
                //throw new FileNotFoundException("Label template not found", templatePath);
                LabelPrintProgress?.Invoke(this, new LabelPrintEventArgs($"Label template not found: {templatePath}", true));
                return;
            }

            _app = new LGApp();
            _label = _app.LabelOpenEx(templatePath);
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
                    LabelPrintProgress?.Invoke(this, new LabelPrintEventArgs("Label template is not loaded. Please load a label template before printing.", true));
                    return false;
                }
                if (string.IsNullOrWhiteSpace(_printerName))
                {
                    LabelPrintProgress?.Invoke(this, new LabelPrintEventArgs("Printer name is not set. Please check your printer connection and configuration.", true));
                    return false;
                }

                _label.PrinterName = _printerName;
                _label.PrinterPort = dirPrn; // LabelGallery uses this as the output directory for file printing

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
                LabelPrintProgress?.Invoke(this, new LabelPrintEventArgs($"Error during print process: {ex.Message}", true));
                return false;
            }
        }
        public void Print(int timeoutSeconds = 300)
        {
            if (!File.Exists(_prnFilePath))
            {
                LabelPrintProgress?.Invoke(this, new LabelPrintEventArgs($"PRN file not found: {_prnFilePath}", true));
                return;
            }

            if (_printer is null)
            {
                LabelPrintProgress?.Invoke(this, new LabelPrintEventArgs("Printer not initialized. Please initialize the printer before printing.", true));
                return;
            }

            try
            {
                byte[] prnData = File.ReadAllBytes(_prnFilePath);
                _printWaitHandle.Reset(); // reset before sending
                _printer.Send(prnData);

                LabelPrintProgress?.Invoke(this, new LabelPrintEventArgs($"PRN file sent to printer: {_printerName}. Waiting for response..."));

                bool signaled = _printWaitHandle.Wait(TimeSpan.FromSeconds(timeoutSeconds));

                if (!signaled)
                {
                    LabelPrintProgress?.Invoke(this, new LabelPrintEventArgs($"Printer did not respond within {timeoutSeconds} seconds.", true));
                    throw new TimeoutException("Printer did not respond in time.");
                }

                Console.WriteLine("Printer responded successfully.");

            }
            catch (Exception ex)
            {
                LabelPrintProgress?.Invoke(this, new LabelPrintEventArgs($"Error during printing: {ex.Message}", true));
                throw;
            }
        }
        #endregion

    }


}
