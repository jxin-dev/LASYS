using System.IO.Ports;
using System.Management;
using System.Runtime.InteropServices;
using System.Text.Json;
using Interop.LabelGalleryPlus3WR;
using LASYS.Application.Common.Extensions;
using LASYS.Application.Common.Messaging;
using LASYS.Application.Contracts;
using LASYS.Application.Events;
using LASYS.Application.Features.Devices.Enums;
using LASYS.Application.Features.Devices.Events;
using LASYS.Application.Features.Devices.Factories;
using LASYS.Application.Features.Devices.Models;
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
        public string? PrinterName => _printerName;
        private string _prnFilePath = string.Empty;
        private bool _isByteAvailableSubscribed;

        public event EventHandler<PrinterNotificationEventArgs>? PrinterNotification;
        public event EventHandler<DeviceStatusChangedEventArgs>? DeviceStatusChanged;
        public event EventHandler<LabelEventArgs>? LabelStatusChanged;

        public string FilePath => _prnFilePath;

        public DeviceStatus CurrentStatus { get; private set; } =
            DeviceStatusFactory.Create(DeviceType.Printer, DeviceStatusCode.NotConfigured);

        private PrinterConnection? _connection;
        public PrinterConnection? Connection => _connection;

        private void SetStatus(DeviceStatusCode statusCode, string? descriptionOverride = null)
        {
            CurrentStatus = DeviceStatusFactory.Create(
                DeviceType.Printer,
                statusCode,
                descriptionOverride);

            DeviceStatusChanged?.Invoke(this, new DeviceStatusChangedEventArgs(CurrentStatus));

        }

        private string ControlCharConvert(string data)
        {
            Dictionary<char, string> chrList = ControlCharList().ToDictionary(x => x.Value, x => x.Key);
            foreach (char key in chrList.Keys)
            {
                data = data.Replace(key.ToString(), chrList[key]);
            }
            return data;
        }

        private Dictionary<string, char> ControlCharList()
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
                        //_printer.ByteAvailable -= AsynDataIn;
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
                    SetStatus(DeviceStatusCode.ConfigurationLoaded, $"Printer configuration loaded successfully.\nCommunication will use serial interface on port {serial.ComPort} with settings {serial.Parameters} at {serial.BaudRate} baud.");
                }
                else if (config?.SatoPrinter is UsbPrinterConnection usb)
                {
                    SetStatus(DeviceStatusCode.ConfigurationLoaded, $"Printer configuration loaded successfully.\nCommunication will use the USB interface with device ID {usb.UsbId.Ellipsis(18)}");
                }
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
                SetStatus(DeviceStatusCode.NotConfigured);
                return;
            }

            _connection = config.SatoPrinter;

            if (_printer == null)
            {
                _printer = new Printer();
                _printer.PermanentConnect = true;
            }


            if (config.SatoPrinter is SerialPrinterConnection serial)
            {

                _printer.Interface = Printer.InterfaceType.COM;
                _printer.COMPort = serial.ComPort;
                _printer.COMSetting.Baudrate = serial.BaudRate;
                _printer.COMSetting.Parameters = serial.Parameters;

                _printerName = GetPrinterNameByComPort(_printer.COMPort);
                if (_printerName == null)
                {
                    SetStatus(DeviceStatusCode.NotDetected, $"No printer detected on COM port {_printer.COMPort}.");
                    return;
                }

                try
                {
                    //_printer.ByteAvailable += AsynDataIn;
                    _printer.Connect();
                    //SetStatus(DeviceStatusCode.Connected, $"Serial COM printer connected: {_printerName} (Port: {serial.ComPort})");
                    bool isOnline = _printer.GetPrinterStatus().IsOnline;
                    if (isOnline)
                    {
                        SetStatus(DeviceStatusCode.Connected, $"Printer is already online: {_printerName ?? "Unknown Printer"}");
                    }
                    else
                    {
                        SetStatus(DeviceStatusCode.Offline, $"Printer is currently offline: {_printerName ?? "Unknown Printer"}. Please check the connection and try again.");
                    }
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
                if (usbPrinter != null && _printer != null)
                {
                    _printerName = usbPrinter.Name; // Get the friendly name
                    _printer.Connect();
                    //SetStatus(DeviceStatusCode.Connected, $"USB printer connected: {_printerName}");
                    bool isOnline = _printer.GetPrinterStatus().IsOnline;
                    if (isOnline)
                    {
                        SetStatus(DeviceStatusCode.Online, $"Printer is already online: {_printerName ?? "Unknown Printer"}");
                    }
                    else
                    {
                        SetStatus(DeviceStatusCode.Offline, $"Printer is currently offline: {_printerName ?? "Unknown Printer"}. Please check the connection and try again.");
                    }
                }
                else
                {
                    //SetStatus(DeviceStatusCode.Disconnected, $"USB printer {usb.UsbId} not connected.");
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
                SetStatus(DeviceStatusCode.NotConfigured);
                return;
            }
            try
            {
                _printer.TestPrint();
            }
            catch (Exception ex)
            {
                SetStatus(DeviceStatusCode.Error, $"Test print failed: {ex.Message}");
            }
        }
        public async Task<bool> IsPrinted(string prnFilePath, int timeoutSeconds = 300)
        {
            if (!File.Exists(prnFilePath))
            {
                SetStatus(DeviceStatusCode.Error, $"PRN file not found: {prnFilePath}.");
                return await Task.FromResult(false);
            }

            if (_printer is null)
            {
                SetStatus(DeviceStatusCode.NotConfigured, "Printer not initialized. Please initialize the printer before printing.");
                return await Task.FromResult(false);
            }

            try
            {


                bool isOnline = _printer.GetPrinterStatus().IsOnline;

                if (isOnline)
                {
                    byte[] prnData = await File.ReadAllBytesAsync(prnFilePath);
                    _printer.Send(prnData);
                    SetStatus(DeviceStatusCode.Online, $"Printer is online. Sending PRN file to printer: {_printerName}...");
                    //SetStatus(DeviceStatusCode.PrinterError, $"Printer reported an error after sending PRN file: {_printerName}. Please check the printer status and try again.");
                    return await Task.FromResult(true);
                }
                else
                {
                    SetStatus(DeviceStatusCode.Offline, $"Printer is offline. Cannot send PRN file: {_printerName}. Please check the connection and try again.");
                    return await Task.FromResult(false);
                }
            }
            catch (Exception ex)
            {
                SetStatus(DeviceStatusCode.Error, $"Error during printing: {ex.Message}");
                return await Task.FromResult(false);
            }
        }
        private string ControlCharReplace(string data)
        {
            Dictionary<string, char> chrList = ControlCharList();
            foreach (string key in chrList.Keys)
            {
                data = data.Replace(key, chrList[key].ToString());
            }
            return data;
        }

    }


}
