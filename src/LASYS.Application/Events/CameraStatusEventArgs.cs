using LASYS.Application.Common.Enums;

namespace LASYS.Application.Events
{
    public sealed class CameraStatusEventArgs : EventArgs
    {
        public CameraStatus Status { get; }
        public string Message { get; }
        public string Description { get; }
        public CameraStatusEventArgs(CameraStatus status, string? description = null)
        {
            Status = status;
            var info = GetStatusInfo(status);
            Message = info.Message;
            Description = description ?? info.Description;
        }
        private (string Message, string Description) GetStatusInfo(CameraStatus status) => status switch
        {
            CameraStatus.CameraConfiguring => ("Configuring...", "The system is applying camera settings from the configuration."),
            CameraStatus.CameraNotConfigured => ("Not Configured", "Camera configuration is missing."),
            CameraStatus.CameraNotDetected => ("Not Detected", "No camera device was found using the configured device name."),
            CameraStatus.CameraConnected => ("Connected", "The camera is connected and ready."),
           CameraStatus.CameraFocusing => ("Focusing...", "The camera is adjusting focus to obtain a clear image."),
            CameraStatus.CameraCapturing =>("Capturing...", "The system is capturing a frame from the camera."),
            CameraStatus.CameraReconnecting => ("Reconnecting...", "The system is attempting to reconnect to the camera."),
            CameraStatus.CameraDisconnected => ("Disconnected", "The camera is not connected."),
            CameraStatus.CameraTimeout => ("Timeout", "The camera did not respond within the expected time."),
            CameraStatus.CameraError => ("Error", "An error occurred while communicating with the camera."),
            _ => ("Unknown Status", "The camera status is unknown.")
        };
    }
}
