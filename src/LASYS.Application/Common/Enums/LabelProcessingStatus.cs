namespace LASYS.Application.Common.Enums
{
    public enum LabelProcessingStatus
    {
        Ready,
        Printing,
        CameraUnavailable,
        Capturing,
        ScanningBarcode,
        ReadingOcr,
        WaitingForOperator,
        Completed,
        Stopped,
        Busy,
        Error,

        CalibrationNotFound,
        ProductNotConfigured
    }
}
