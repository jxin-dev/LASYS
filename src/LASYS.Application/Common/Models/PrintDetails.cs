namespace LASYS.Application.Common.Models
{
    public sealed record PrintDetails
    {
        public string ItemCode { get; init; } = string.Empty;
        public string LotNo { get; init; } = string.Empty;
        public long TotalPassed { get; init; }
        public long TotalFailed { get; init; }
        public long TotalSampled { get; init; }
        public long TotalPrinted { get; init; }
        public long NextSequence { get; init; }
        public long BatchNumber { get; init; }
        public long SetNumber { get; init; }
        public long GetRemainingPrintQuantity(uint? targetPrintQuantity) => (targetPrintQuantity ?? 0) - TotalPrinted;

        public PrintDetails IncrementPassed() => this with
        {
            TotalPassed = TotalPassed + 1
        };

        public PrintDetails IncrementFailed() => this with
        {
            TotalFailed = TotalFailed + 1
        };

        public PrintDetails IncrementSampled() => this with
        {
            TotalSampled = TotalSampled + 1
        };

        public PrintDetails IncrementPrinted() => this with
        {
            TotalPrinted = TotalPrinted + 1
        };
        public PrintDetails WithNextSequenceNumber(long nextSequence) => this with
        {
            NextSequence = nextSequence
        };

        public PrintDetails WithBatchNumber(long batchNumber) => this with
        {
            BatchNumber = batchNumber
        };

        public PrintDetails WithSetNumber(long setNumber) => this with
        {
            SetNumber = setNumber
        };
    }
}
