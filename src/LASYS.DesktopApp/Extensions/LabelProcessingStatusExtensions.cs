using LASYS.Application.Common.Enums;

namespace LASYS.DesktopApp.Extensions
{
    public static class LabelProcessingStatusExtensions
    {
        public static string ToDisplayText(this LabelProcessingStatus status)
        {
            return status switch
            {
                LabelProcessingStatus.Ready => "System Ready.",
                LabelProcessingStatus.Printing => "Printing label...",
                LabelProcessingStatus.Capturing => "Capturing image from camera...",
                LabelProcessingStatus.ScanningBarcode => "Scanning barcode...",
                LabelProcessingStatus.ReadingOcr => "Processing text recognition...",
                LabelProcessingStatus.WaitingForOperator => "Awaiting operator action...",
                LabelProcessingStatus.Completed => "Label printed successfully.",
                LabelProcessingStatus.Stopped => "Operation stopped by operator.",
                LabelProcessingStatus.Busy => "A label processing job is already in progress.",
                LabelProcessingStatus.Error => "An unexpected error occurred.",
                LabelProcessingStatus.CalibrationNotFound => "No calibration data available.",
                LabelProcessingStatus.ProductNotConfigured => "Selected item is not configured in calibration data.",
                _ => status.ToString()
            };
        }
    }
}
