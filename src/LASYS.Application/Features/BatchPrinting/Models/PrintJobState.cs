using LASYS.Application.Common.Mappings;
using LASYS.Application.Common.Models;
using LASYS.Application.Common.Utilities;
using LASYS.Application.Features.BatchPrinting.Enums;
using LASYS.Application.Features.BatchPrinting.Helpers;
using LASYS.Application.Features.LabelInstructions.GetLabelInstructionContext;

namespace LASYS.Application.Features.BatchPrinting.Models
{
    public sealed class PrintJobState
    {
        public Guid JobId { get; private set; }
        public string PrinterName { get; private set; } = default!;
        public LabelPrintingContext Context { get; private set; } = default!;
        public string ItemCode => Context.LabelInstructionDetails?.ItemCode ?? throw new InvalidOperationException("ItemCode is not available.");
        public string LotNo => Context.LabelInstructionDetails?.LotNo ?? throw new InvalidOperationException("LotNo is not available.");
        public string BoxType => Context.MasterLabelDetails?.BoxType.ToString() ?? throw new InvalidOperationException("BoxType is not available.");
        public bool IsPairedType => Context.ProductDetails?.IsPairedBoxType ?? throw new InvalidOperationException("IsPairedBoxType is not available.");
        public int SequenceLength { get; init; } = 6;
        public string NiceLabelFilePath => Context.MasterLabelDetails?.FilePath ?? throw new InvalidOperationException("NiceLabelFilePath is not available.");
        public NiceLabelVariableCollection LabelData => Context is not null ? NiceLabelDataMappings.ToLabelData(Context) : throw new InvalidOperationException("LabelData is not available.");
        public int TotalQuantity { get; private set; }
        public int PrintedCount { get; private set; } = 0;
        public int BatchNumber => Context.PrintDetails?.BatchNumber != null ? (int)Context.PrintDetails.BatchNumber : throw new InvalidOperationException("BatchNumber is not available.");
        public int SetNumber => Context.PrintDetails?.SetNumber != null ? (int)Context.PrintDetails.SetNumber : throw new InvalidOperationException("SetNumber is not available.");
        public int StartSequence => Context.PrintDetails?.NextSequence != null ? (int)Context.PrintDetails.NextSequence : throw new InvalidOperationException("StartSequence is not available.");
        public int CurrentSequence => StartSequence + PrintedCount;
        public string CurrentSequenceFormat => SequenceFormatter.Format(CurrentSequence, SequenceLength);
        public string LastSequenceFormat => SequenceFormatter.Format((StartSequence + TotalQuantity) - 1, SequenceLength);
        public PrintJobStatus Status { get; private set; } = PrintJobStatus.Pending;
        public PrintJobPaths Paths { get; init; } = default!;

        // CONTROL PRIMITIVES
        public ManualResetEventSlim ResumeSignal { get; } = new(true);
        public CancellationTokenSource CancellationTokenSource { get; } = new();


        public static PrintJobState Create(string printerName, LabelPrintingContext context, int quantityToPrint)
        {
            return new PrintJobState
            {
                JobId = Guid.NewGuid(),
                PrinterName = printerName,
                Context = context,
                TotalQuantity = quantityToPrint,
                Paths = PrintJobPathBuilder.Create(
                    context.LabelInstructionDetails?.ItemCode ?? throw new InvalidOperationException("ItemCode is not available."),
                    context.LabelInstructionDetails?.LotNo ?? throw new InvalidOperationException("LotNo is not available."),
                    context.MasterLabelDetails?.BoxType.ToString() ?? throw new InvalidOperationException("BoxType is not available."))
            };
        }

        public void Ready()
        {
            Status = PrintJobStatus.Ready;
        }
        public void InProgress()
        {
            Status = PrintJobStatus.InProgress;
        }
        public void Paused()
        {
            Status = PrintJobStatus.Paused;
        }
        public void Stopped()
        {
            Status = PrintJobStatus.Stopped;
        }
        public void Completed()
        {
            Status = PrintJobStatus.Completed;
        }
        public void Failed()
        {
            Status = PrintJobStatus.Failed;
        }
        public void IncrementPrintedCount()
        {
            PrintedCount++;
        }

    }
}
