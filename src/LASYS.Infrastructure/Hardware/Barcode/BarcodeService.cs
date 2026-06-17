using System.Diagnostics;
using System.IO.Ports;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using LASYS.Application.Common.Messaging;
using LASYS.Application.Contracts;
using LASYS.Application.Events;
using LASYS.Application.Features.Devices.Enums;
using LASYS.Application.Features.Devices.Events;
using LASYS.Application.Features.Devices.Factories;
using LASYS.Application.Features.Devices.Models;
using LASYS.Application.Interfaces.Services;
using Newtonsoft.Json;

namespace LASYS.Infrastructure.Hardware.Barcode
{
    public class BarcodeService : IBarcodeService
    {
        private readonly string _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "barcode.config.json");

        private readonly object _syncRoot = new();
        private readonly object _scanLock = new();

        private SerialPort? _port;
        private readonly StringBuilder _buffer = new();

        public DeviceStatus CurrentStatus { get; private set; } = DeviceStatusFactory.Create(DeviceType.BarcodeScanner, DeviceStatusCode.NotConfigured);

        private TaskCompletionSource<string?>? _scanTcs;

        private readonly byte[] TRIGGER_ON = [0x7E, 0x80, 0x02, 0x00, 0x00, 0x00, 0x01, 0x01, 0x82, 0x7E];
        private readonly byte[] TRIGGER_OFF = [0x7E, 0x80, 0x02, 0x00, 0x00, 0x00, 0x01, 0x00, 0x83, 0x7E];

        public event EventHandler<BarcodeNotificationEventArgs>? BarcodeNotification;
        public event EventHandler<BarcodeStatusEventArgs>? BarcodeStatusChanged;
        public event EventHandler<BarcodeScannedEventArgs>? BarcodeScanned;
        public event EventHandler<DeviceStatusChangedEventArgs>? DeviceStatusChanged;


        private void SetStatus(DeviceStatusCode statusCode, string? descriptionOverride = null)
        {
            CurrentStatus = DeviceStatusFactory.Create(
                DeviceType.BarcodeScanner,
                statusCode,
                descriptionOverride);

            DeviceStatusChanged?.Invoke(
                this,
                new DeviceStatusChangedEventArgs(CurrentStatus));
        }
        public async Task InitializeAsync()
        {
            var config = await LoadAsync();
            if (config == null || string.IsNullOrWhiteSpace(config.Port))
            {
                SetStatus(DeviceStatusCode.NotConfigured);
                //BarcodeStatusChanged?.Invoke(this, new BarcodeStatusEventArgs(BarcodeStatus.BarcodeNotConfigured));
                return;
            }

            lock (_syncRoot)
            {
                if (_port != null && _port.PortName.Equals(config.Port, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (_port != null)
                {
                    _port.DataReceived -= OnDataReceived;

                    if (_port.IsOpen)
                        _port.Close();

                    _port.Dispose();
                    _port = null;
                }
            }

            _port = new SerialPort(config.Port, config.BaudRate, Parity.None, 8, StopBits.One)
            {
                Handshake = Handshake.None,
                Encoding = Encoding.ASCII,
                ReadTimeout = 500,
                WriteTimeout = 500,
                NewLine = "\r\n"
            };

            _port.DataReceived += OnDataReceived;

            try
            {
                lock (_syncRoot)
                {
                    if (!_port.IsOpen)
                        _port.Open();
                }

                SetStatus(DeviceStatusCode.Connected, $"The barcode scanner was initialized on port {config.Port}.");

                //BarcodeStatusChanged?.Invoke(this,new BarcodeStatusEventArgs(
                //         BarcodeStatus.BarcodeConnected,
                //         $"The barcode scanner was initialized on port {config.Port}."));
            }
            catch
            {
                SetStatus(DeviceStatusCode.NotDetected, $"Barcode scanner not detected on port {config.Port}.");
                //BarcodeStatusChanged?.Invoke(this, new BarcodeStatusEventArgs(BarcodeStatus.BarcodeNotDetected, $"Barcode scanner not detected on port {config.Port}."));
            }
        }
        public bool IsConnected
        {
            get
            {
                lock (_syncRoot)
                {
                    return _port != null && _port.IsOpen;
                }
            }
        }


        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (_port == null || !_port.IsOpen)
                    return;

                var incoming = _port.ReadExisting();

                if (string.IsNullOrEmpty(incoming))
                    return;

                _buffer.Append(incoming);

                while (true)
                {
                    var content = _buffer.ToString();

                    var index = content.IndexOfAny(new[] { '\r', '\n' });

                    if (index < 0)
                        break;

                    var barcode = content.Substring(0, index).Trim();

                    _buffer.Remove(0, index + 1);

                    while (_buffer.Length > 0 &&
                           (_buffer[0] == '\r' || _buffer[0] == '\n'))
                    {
                        _buffer.Remove(0, 1);
                    }

                    if (string.IsNullOrWhiteSpace(barcode))
                        continue;

                    barcode = Regex.Replace(barcode, @"^[^0-9]+", string.Empty);

                    if (_scanTcs == null || _scanTcs.Task.IsCompleted)
                        break;

                    StopScan();

                    if (_scanTcs.TrySetResult(barcode))
                    {
                        BarcodeScanned?.Invoke(
                            this,
                            new BarcodeScannedEventArgs(barcode));

                        SetStatus(
                            DeviceStatusCode.Connected,
                            $"Scanned barcode: {barcode}");
                    }

                    break;
                }
            }
            catch (Exception ex)
            {
                StopScan();

                _scanTcs?.TrySetException(ex);

                SetStatus(DeviceStatusCode.Error);
            }
        }

        public async Task ScanAsync()
        {
            if (_port == null)
            {
                SetStatus(DeviceStatusCode.Disconnected);
                return;
            }

            try
            {
                lock (_syncRoot)
                {
                    if (!_port.IsOpen)
                        _port.Open();

                    _port.DiscardInBuffer();
                    _port.DiscardOutBuffer();

                    _port.BaseStream.Write(
                        TRIGGER_ON,
                        0,
                        TRIGGER_ON.Length);
                }

                SetStatus(DeviceStatusCode.Scanning);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(
                    $"Failed to trigger scan: {ex.Message}");

                SetStatus(DeviceStatusCode.Error);

                _scanTcs?.TrySetException(ex);
            }
        }
        //
        // TODO: Replace with real command from scanner manual
        private readonly byte[] CMD_SET_MANUAL_MODE = new byte[] { 0x16, 0x4D };
        // Optional if available:
        //private readonly byte[] CMD_SET_MANUAL_MODE = Encoding.ASCII.GetBytes("SETTRIGGER=MANUAL\r\n");

        // TODO: Replace with real command from scanner manual
        private readonly byte[] CMD_SAVE_SETTINGS = new byte[] { 0x16, 0x53 };
        private async Task<bool> SendScannerCommandAsync(byte[] command, int delayAfterMs = 200)
        {
            if (_port == null)
            {
                SetStatus(DeviceStatusCode.Disconnected);
                //BarcodeStatusChanged?.Invoke(this, new BarcodeStatusEventArgs(BarcodeStatus.BarcodeDisconnected));
                return false;
            }
            try
            {
                if (!_port.IsOpen) _port.Open();
                // Clear old data
                _port.DiscardInBuffer();
                _port.DiscardOutBuffer();

                // Send command
                await _port.BaseStream.WriteAsync(command, 0, command.Length);
                await _port.BaseStream.FlushAsync();

                if (delayAfterMs > 0)
                    await Task.Delay(delayAfterMs);


                return true;
            }
            catch (Exception)
            {
                SetStatus(DeviceStatusCode.Error);
                //BarcodeStatusChanged?.Invoke(this, new BarcodeStatusEventArgs(BarcodeStatus.BarcodeError));
                return false;
            }
        }
        public async Task SetManualModeAsync()
        {
            //BarcodeStatusChanged?.Invoke(this, new BarcodeStatusEventArgs(BarcodeStatus.BarcodeCommunicating));
            SetStatus(DeviceStatusCode.Communicating);
            await SendScannerCommandAsync(CMD_SET_MANUAL_MODE);
            await SendScannerCommandAsync(CMD_SAVE_SETTINGS);
            // To verify, barcode light should turn off after this command,
            // indicating manual mode is active. If not, check scanner manual for correct command bytes.
            //BarcodeStatusChanged?.Invoke(this, new BarcodeStatusEventArgs(BarcodeStatus.BarcodeConnected));
            SetStatus(DeviceStatusCode.Connected, "Scanner set to manual mode.");

        }


        //
        public async Task<BarcodeConfig?> LoadAsync()
        {
            try
            {
                if (!File.Exists(_configPath))
                {
                    //BarcodeStatusChanged?.Invoke(this, new BarcodeStatusEventArgs(BarcodeStatus.BarcodeNotConfigured));
                    SetStatus(DeviceStatusCode.NotConfigured);
                    return null;
                }

                var json = await File.ReadAllTextAsync(_configPath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    SetStatus(DeviceStatusCode.NotConfigured);
                    //BarcodeStatusChanged?.Invoke(this, new BarcodeStatusEventArgs(BarcodeStatus.BarcodeNotConfigured, "Configuration file is empty."));
                    return null;
                }

                var config = JsonConvert.DeserializeObject<BarcodeConfig>(json);
                if (config == null)
                {
                    SetStatus(DeviceStatusCode.NotConfigured, "Invalid configuration format.");
                    //BarcodeStatusChanged?.Invoke(this, new BarcodeStatusEventArgs(BarcodeStatus.BarcodeNotConfigured, "Invalid configuration format."));
                    return null;
                }

                //var portMessage = string.IsNullOrWhiteSpace(config.Port)
                //       ? "Barcode configuration loaded (no port configured)."
                //       : $"Barcode configuration loaded successfully. Port: {config.Port}";

                //BarcodeStatusChanged?.Invoke(this, new BarcodeStatusEventArgs(BarcodeStatus.BarcodeConnected, portMessage));
                return config;
            }
            catch (Exception)
            {
                SetStatus(DeviceStatusCode.Error, "Failed to load configuration.");
                //BarcodeStatusChanged?.Invoke(this, new BarcodeStatusEventArgs(BarcodeStatus.BarcodeError));
                return null;
            }
        }
        public async Task SaveAsync(BarcodeConfig config)
        {
            try
            {
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                await File.WriteAllTextAsync(_configPath, json);
                BarcodeNotification?.Invoke(this, new BarcodeNotificationEventArgs(MessageType.Info, "Barcode configuration saved successfully."));
                await InitializeAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to save config: {ex.Message}");
                BarcodeNotification?.Invoke(this, new BarcodeNotificationEventArgs(MessageType.Error, $"Failed to save config: {ex.Message}"));
                throw;
            }
        }

        public IReadOnlyList<string> GetUSBVirtualCOMPortList()
        {
            var ports = new List<string>();
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Non-Windows fallback
                return SerialPort.GetPortNames().ToList();
            }

            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%Cino FuzzyScan USB Virtual COM Port%'"))
            {
                foreach (var obj in searcher.Get())
                {
                    var name = obj["Name"]?.ToString();
                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    var match = Regex.Match(name, @"COM\d+");

                    if (match.Success)
                    {
                        ports.Add(match.Value);
                    }
                }
            }

            return ports;
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

        public async Task<string?> WaitForBarcodeAsync(CancellationToken token)
        {
            _scanTcs = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);

            try
            {
                var completedTask = await Task.WhenAny(
                    _scanTcs.Task,
                    Task.Delay(TimeSpan.FromSeconds(10), token));

                if (completedTask != _scanTcs.Task)
                {
                    StopScan();
                    SetStatus(DeviceStatusCode.Timeout);
                    return null;
                }

                return await _scanTcs.Task;
            }
            catch (OperationCanceledException)
            {
                StopScan();
                throw;
            }
            finally
            {
                _scanTcs = null;
            }
        }
        private void StopScan()
        {
            try
            {
                lock (_syncRoot)
                {
                    if (_port?.IsOpen == true)
                    {
                        _port.BaseStream.Write(
                            TRIGGER_OFF,
                            0,
                            TRIGGER_OFF.Length);
                    }
                }
            }
            catch
            {
                // Ignore scanner shutdown errors
            }
        }
    }
}
