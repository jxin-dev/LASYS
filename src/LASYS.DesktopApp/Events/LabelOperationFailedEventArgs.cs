using LASYS.DesktopApp.Enums;

namespace LASYS.DesktopApp.Events
{
    public class LabelOperationFailedEventArgs : EventArgs
    {
        public string? CustomMessage { get; }  // Optional custom message
        public LabelOperationType OperationType { get; }
        public int? SequenceNo { get; }
        public string? OcrResult { get; }
        public string? BarcodeResult { get; }

        public LabelOperationFailedEventArgs(
            LabelOperationType operationType,
            string? customMessage = null,
            int? sequenceNo = null,
            string? ocrResult = null,
            string? barcodeResult = null)
        {
            OperationType = operationType;
            CustomMessage = customMessage;
            SequenceNo = sequenceNo;
            OcrResult = ocrResult;
            BarcodeResult = barcodeResult;
        }
    }
}
