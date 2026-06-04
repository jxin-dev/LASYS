using System.Data;
using LASYS.Application.Features.Devices.Enums;
using LASYS.Application.Features.Devices.Models;
using static OpenCvSharp.FileStorage;

namespace LASYS.Application.Features.Devices.Extensions
{
    public static class DeviceStatusExtensions
    {
        public static DeviceStatusInfo GetStatusInfo(this DeviceStatusCode status, DeviceType device)
        {
            return (device, status) switch
            {
                (_, DeviceStatusCode.NotConfigured) =>
                    new("Not Configured", $"{device} configuration is missing."),

                (_, DeviceStatusCode.NotDetected) =>
                    new("Not Detected", $"No {device} device was found."),

                (_, DeviceStatusCode.Connected) =>
                    new("Connected", $"{device} is connected and ready."),

                (_, DeviceStatusCode.Disconnected) =>
                    new("Disconnected", $"{device} is not connected."),

                (_, DeviceStatusCode.Reconnecting) =>
                    new("Reconnecting...", $"The system is attempting to reconnect to {device}."),

                (_, DeviceStatusCode.Timeout) =>
                    new("Timeout", $"{device} did not respond within the expected time."),

                (_, DeviceStatusCode.Error) =>
                    new("Error", $"An error occurred while communicating with {device}."),
                // Specific statuses for Camera
                (DeviceType.Camera, DeviceStatusCode.CameraCapturing) =>
                    new("Capturing...", "The system is capturing a frame from the camera."),
                (DeviceType.Camera, DeviceStatusCode.CameraFocusing) =>
                    new("Focusing...", "The camera is adjusting focus to obtain a clear image."),
                // Specific statuses for BarcodeScanner
                (DeviceType.BarcodeScanner, DeviceStatusCode.Communicating) =>
                    new("Communicating...", "The system is sending a command to the barcode scanner."),
                (DeviceType.BarcodeScanner, DeviceStatusCode.Scanning) =>
                    new("Scanning...", "The system is waiting for a barcode to be scanned."),
                (DeviceType.BarcodeScanner, DeviceStatusCode.Scanned) =>
                    new("Scan Received", "A barcode was successfully scanned."),
                // Specific statuses for Printer
                (DeviceType.Printer, DeviceStatusCode.ConfigurationLoaded) =>
                    new("Printer configuration loaded", "The printer configuration was successfully loaded from the application settings."),
                (DeviceType.Printer, DeviceStatusCode.Started) =>
                    new("Print Started", "The printer has started the print job."),
                (DeviceType.Printer, DeviceStatusCode.Online) =>
                    new("Online", "The printer is online and ready."),
                (DeviceType.Printer, DeviceStatusCode.Offline) =>
                    new("Offline", "The printer is currently offline."),
                (DeviceType.Printer, DeviceStatusCode.Paused) =>
                    new("Paused", "The printer is paused."),
                (DeviceType.Printer, DeviceStatusCode.Resuming) =>
                    new("Resuming...", "The printer is resuming from a paused state."),
                (DeviceType.Printer, DeviceStatusCode.DataSent) =>
                    new("Printer data sent", "The system has transmitted data to the printer and is waiting for a response."),
                (DeviceType.Printer, DeviceStatusCode.DataReceived) =>
                    new("Printer response received", "The system received data from the printer."),
                (DeviceType.Printer, DeviceStatusCode.Connecting) =>
                    new("Connecting...", "The system is attempting to establish a connection with the printer."),
                (DeviceType.Printer, DeviceStatusCode.PrintCompleted) =>
                    new("Print Completed", "The print job has completed successfully."),
                (DeviceType.Printer, DeviceStatusCode.PrintFailed) =>
                    new("Print Failed", "The print job has failed."),
               
                _ =>
                    new("Unknown Status", $"{device} status is unknown.")
            };
        }
    }
}
