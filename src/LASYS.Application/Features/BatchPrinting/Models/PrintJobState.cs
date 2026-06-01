using LASYS.Application.Common.Models;
using LASYS.Application.Features.BatchPrinting.Enums;

namespace LASYS.Application.Features.BatchPrinting.Models
{
    public sealed class PrintJobState
    {
        public Guid JobId { get; init; }
        public string ItemCode { get; init; } = "ItemCode";
        public string LotNo { get; init; } = "LotNo";
        public string BoxType { get; init; } = "BoxType";
        public string NiceLabelFilePath { get; init; } = default!;
        public NiceLabelVariableCollection LabelData { get; init; } = new();
        public int TotalQuantity { get; init; }
        public int PrintedCount { get; set; }
        public int StartSequenceToPrint { get; set; }
        public int LastSequenceToPrint => (StartSequenceToPrint + TotalQuantity) - 1;
        public PrintJobStatus Status { get; set; }
        public PrintJobPaths Paths { get; init; } = default!;

        // CONTROL PRIMITIVES
        public ManualResetEventSlim ResumeSignal { get; } = new(true);
        public CancellationTokenSource CancellationTokenSource { get; } = new();

    }
}
