namespace LASYS.Application.Common.Models
{
    public sealed record PrintDetails
    {
        public string ItemCode { get; set; } = string.Empty;
        public string LotNo { get; set; } = string.Empty;
        public long TotalPassed { get; set; }
        public long TotalFailed { get; set; }
        public long TotalSampled { get; set; }
        public long TotalPrinted { get; set; }
        public long NextSequence { get; set; }
        public long BatchNumber { get; set; }
        public long SetNumber { get; set; }
        public long GetRemainingPrintQuantity(uint? targetPrintQuantity) => ((targetPrintQuantity ?? 0) - TotalPassed);

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
