namespace LASYS.Application.Features.BatchPrinting.Enums
{
    public enum PrintJobStatus
    {
        Initializing,
        Ready,
        InProgress,
        Printing,
        //ScanningBarcode,
        //ReadingOcr,
        //WaitingForBarcodeDecision, 
        //WaitingForOcrDecision,
        Paused,
        Completed,
        Stopped,
        Failed
    }
}
