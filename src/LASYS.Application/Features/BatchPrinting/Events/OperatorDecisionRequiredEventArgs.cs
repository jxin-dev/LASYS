using LASYS.Application.Features.BatchPrinting.Enums;

namespace LASYS.Application.Features.BatchPrinting.Events
{
    public class OperatorDecisionRequiredEventArgs : EventArgs
    {
        public ValidationFailure FailureType { get; init; }
        public int SequenceNo { get; init; }
        public string? BarcodeResult { get; init; }
        public string? OcrResult { get; init; }
        public string? CustomMessage { get; init; }
    }
}
