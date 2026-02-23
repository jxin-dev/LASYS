using System.IO.Ports;
using System.Text;
using LASYS.BarcodeAnalyzer.Events;
using LASYS.BarcodeAnalyzer.Interfaces;
using LASYS.BarcodeAnalyzer.Models;
using Newtonsoft.Json;

namespace LASYS.BarcodeAnalyzer.Services
{
    public class BarcodeService : IBarcodeService
    {
        private readonly string _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "barcode.config.json");

        private readonly object _syncRoot = new object();
        private SerialPort? _port;
        private readonly StringBuilder _buffer = new();
        private volatile bool _isWaitingForScan;

        private readonly byte[] TRIGGER_ON = [0x7E, 0x80, 0x02, 0x00, 0x00, 0x00, 0x01, 0x01, 0x82, 0x7E];
        private readonly byte[] TRIGGER_OFF = [0x7E, 0x80, 0x02, 0x00, 0x00, 0x00, 0x01, 0x00, 0x83, 0x7E];

        public event EventHandler<BarcodeNotificationEventArgs>? BarcodeNotification;
        public event EventHandler<BarcodeStatusEventArgs>? BarcodeStatusChanged;
        public event EventHandler<BarcodeScannedEventArgs>? BarcodeScanned;

        public async Task InitializeAsync()
        {
            var config = await LoadAsync();
            if (config == null || string.IsNullOrWhiteSpace(config.Port))
            {
                BarcodeStatusChanged?.Invoke(this, new BarcodeStatusEventArgs("Barcode scanner not configured. Please set the port in barcode.config.json."));
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
            BarcodeStatusChanged?.Invoke(this, new BarcodeStatusEventArgs($"Barcode initialized on port {config.Port}."));
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (_port == null || !_port.IsOpen) return;

                var incoming = _port.ReadExisting();
                if (string.IsNullOrEmpty(incoming))
                    return;

                _buffer.Append(incoming);

                while (true)
                {
                    var content = _buffer.ToString();
                    var index = content.IndexOf(_port.NewLine, StringComparison.Ordinal);
                    if (index < 0) break;

                    var barcode = content.Substring(0, index).Trim();
                    _buffer.Remove(0, index + _port.NewLine.Length);

                    if (string.IsNullOrWhiteSpace(barcode))
                        continue;

                    // Accept only if scan requested
                    if (!_isWaitingForScan)
                        return;

                    _isWaitingForScan = false;

                    BarcodeScanned?.Invoke(this, new BarcodeScannedEventArgs(barcode));

                    BarcodeStatusChanged?.Invoke(this, new BarcodeStatusEventArgs($"Scan received: {barcode}"));

                    break; // Stop after first valid scan
                }

            }
            catch (Exception ex)
            {
                BarcodeStatusChanged?.Invoke(this, new BarcodeStatusEventArgs($"Barcode read error: {ex.Message}"));
            }
        }

        public async Task ScanAsync()
        {
            if (_isWaitingForScan) // already waiting
            {
                BarcodeStatusChanged?.Invoke(this, new BarcodeStatusEventArgs("Scan already in progress."));
                return;
            }
            if (_port == null)
            {
                BarcodeStatusChanged?.Invoke(this, new BarcodeStatusEventArgs("Barcode scanner not initialized."));
                return;
            }
            try
            {
                _isWaitingForScan = true;
                // Start timeout task immediately after enabling scan
                _ = Task.Run(async () =>
                {
                    await Task.Delay(10000); // 10-second timeout
                    if (_isWaitingForScan)
                    {
                        _isWaitingForScan = false;
                        BarcodeStatusChanged?.Invoke(this,
                            new BarcodeStatusEventArgs("Scan timeout."));
                    }
                });

                // Ensure port is open
                if (!_port.IsOpen) _port.Open();

                // Trigger scanner ON
                await _port.BaseStream.WriteAsync(TRIGGER_ON, 0, TRIGGER_ON.Length);
                await Task.Delay(100); // wait for trigger activation
                await _port.BaseStream.WriteAsync(TRIGGER_OFF, 0, TRIGGER_OFF.Length);

                BarcodeStatusChanged?.Invoke(this, new BarcodeStatusEventArgs("Scan triggered successfully."));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to trigger scan: {ex.Message}");
                BarcodeStatusChanged?.Invoke(this, new BarcodeStatusEventArgs($"Failed to trigger scan: {ex.Message}"));
                _isWaitingForScan = false; // reset flag on error
            }
        }

        public async Task<BarcodeConfig?> LoadAsync()
        {
            try
            {
                if (!File.Exists(_configPath))
                {
                    BarcodeStatusChanged?.Invoke(this, new BarcodeStatusEventArgs("Configuration file not found."));
                    return null;
                }

                var json = await File.ReadAllTextAsync(_configPath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    BarcodeStatusChanged?.Invoke(this, new BarcodeStatusEventArgs("Configuration file is empty."));
                    return null;
                }

                var config = JsonConvert.DeserializeObject<BarcodeConfig>(json);
                if (config == null)
                {
                    BarcodeStatusChanged?.Invoke(this, new BarcodeStatusEventArgs("Invalid configuration format."));
                    return null;
                }

                var portMessage = string.IsNullOrWhiteSpace(config.Port)
                       ? "Barcode configuration loaded (no port configured)."
                       : $"Barcode configuration loaded successfully. Port: {config.Port}";

                BarcodeStatusChanged?.Invoke(this, new BarcodeStatusEventArgs(portMessage));
                return config;
            }
            catch (Exception ex)
            {
                BarcodeStatusChanged?.Invoke(this, new BarcodeStatusEventArgs("Failed to load barcode.config.json"));
                return null;
            }
        }
        public async Task SaveAsync(BarcodeConfig config)
        {
            try
            {
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                await File.WriteAllTextAsync(_configPath, json);
                BarcodeNotification?.Invoke(this, new BarcodeNotificationEventArgs(BarcodeMessageType.Info, "Barcode configuration saved successfully."));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to save config: {ex.Message}");
                BarcodeNotification?.Invoke(this, new BarcodeNotificationEventArgs(BarcodeMessageType.Error, $"Failed to save config: {ex.Message}"));
                throw;
            }
        }


    }
}
