
using LASYS.Application.Common.Enums;

namespace LASYS.Application.Events
{
    public class OperatorDecisionRequiredEventArgs : EventArgs
    {
        public string? CustomMessage { get; }  // Optional custom message
        public ValidationFailure FailureType { get; }
        public string? SequenceNo { get; }
        public string? OcrResult { get; }
        public string? BarcodeResult { get; }

        public OperatorDecisionRequiredEventArgs(
            ValidationFailure operationType,
            string? customMessage = null,
            string? sequenceNo = null,
            string? ocrResult = null,
            string? barcodeResult = null)
        {
            FailureType = operationType;
            CustomMessage = customMessage;
            SequenceNo = sequenceNo;
            OcrResult = ocrResult;
            BarcodeResult = barcodeResult;
        }
    }
}
