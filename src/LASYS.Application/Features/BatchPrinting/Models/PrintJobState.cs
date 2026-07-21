using LASYS.Application.Common.Enums;
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
        public ProcessingStage CurrentStage { get; private set; } = ProcessingStage.None;
        public bool EndOfBatch { get; private set; } = false;
        public bool RequiresApproval => CurrentStage >= ProcessingStage.Printed;
        public string CurrentLabelStatus { get; set; } = default!;
        public string PrinterName { get; private set; } = default!;
        public int CurrentPairNumber { get; private set; }
        public int CurrentPairCount { get; private set; }
        public LabelPrintingContext Context { get; private set; } = default!;
        public string ItemCode => Context.LabelInstructionDetails?.ItemCode ?? throw new InvalidOperationException("ItemCode is not available.");
        public string LotNo => Context.LabelInstructionDetails?.LotNo ?? throw new InvalidOperationException("LotNo is not available.");
        public uint Revision => Context.LabelInstructionDetails?.MasterLabelRevNumber ?? throw new InvalidOperationException("MasterLabelRevNumber is not available.");
        public BoxType BoxType => Context.MasterLabelDetails?.BoxType ?? throw new InvalidOperationException("BoxType is not available.");
        public bool IsPairedType => Context.ProductDetails?.IsPairedBoxType ?? throw new InvalidOperationException("IsPairedBoxType is not available.");
        public int SequenceLength { get; init; } = 6;
        public string NiceLabelFilePath => Context.MasterLabelDetails?.FilePath ?? throw new InvalidOperationException("NiceLabelFilePath is not available.");
        public NiceLabelVariableCollection LabelData => Context is not null ? NiceLabelDataMappings.ToLabelData(Context) : throw new InvalidOperationException("LabelData is not available.");
        public int TotalQuantity { get; private set; }
        public int PrintedCount { get; private set; } = 0;
        public uint TargetQuantity => Context.ProductDetails?.Quantity ?? throw new InvalidOperationException("TargetProductionQuantity is not available");
        public long RemainingQuantity => Context.PrintDetails != null ? Context.PrintDetails.GetRemainingPrintQuantity(Context.ProductDetails?.Quantity) : throw new InvalidOperationException("RemainingQuantity is not available.");
        public long DisplaySequence => RemainingQuantity == 0 ? Context.PrintDetails!.NextSequence - 1 : Context.PrintDetails!.NextSequence;
        public string CurrentSequenceFormat => SequenceFormatter.Format(Context.PrintDetails!.NextSequence, SequenceLength);
        public string LastSequenceFormat => SequenceFormatter.Format((Context.PrintDetails!.NextSequence + TotalQuantity) - 1, SequenceLength);
        public PrintJobStatus Status { get; private set; } = PrintJobStatus.Pending;
        public PrintJobPaths Paths { get; init; } = default!;

        // CONTROL PRIMITIVES
        public ManualResetEventSlim ResumeSignal { get; } = new(true);
        public CancellationTokenSource CancellationTokenSource { get; private set; } = new();
        public string? ApprovedByUserCode { get; private set; }
        public string? ApprovedBySectionId { get; private set; }
        public string? ApprovedByIpAddress { get; private set; }
        public string? ApprovedByDateTime { get; private set; }

        public static PrintJobState Create(string printerName, LabelPrintingContext context)
        {

            var remaining = context.PrintDetails != null ? context.PrintDetails.GetRemainingPrintQuantity(context.ProductDetails?.Quantity) : throw new InvalidOperationException("RemainingQuantity is not available.");
            var startSequence = context.PrintDetails?.NextSequence != null ? (int)context.PrintDetails.NextSequence : throw new InvalidOperationException("NextSequence is not available.");
            var setNumber = context.PrintDetails?.SetNumber != null ? (int)context.PrintDetails.SetNumber : throw new InvalidOperationException("SetNumber is not available.");

            //var batchNumber = context.PrintDetails.TotalPassed == 0 ? 1 : (int)((context.PrintDetails.TotalPassed - 1) / context.ProductDetails!.BatchSize) + 1;
            var batchNumber = context.PrintDetails?.BatchNumber != null ? (int)context.PrintDetails.BatchNumber : throw new InvalidOperationException("BatchNumber is not available.");

            context.PrintDetails.NextSequence = remaining == 0 ? --startSequence : startSequence;
            context.PrintDetails.SetNumber = remaining == 0 ? --setNumber : setNumber;
            context.PrintDetails.BatchNumber = remaining == 0 ? --batchNumber : batchNumber;

            return new PrintJobState
            {
                JobId = Guid.NewGuid(),
                PrinterName = printerName,
                Context = context,
                CurrentLabelStatus = context.LabelInstructionDetails!.PrintType,
                Paths = PrintJobPathBuilder.Create(
                    context.LabelInstructionDetails!.ItemCode,
                    context.LabelInstructionDetails!.LotNo,
                    context.MasterLabelDetails!.BoxType.ToString()),
                Status = remaining == 0 ? PrintJobStatus.Printed : PrintJobStatus.Ready
            };
        }

        public void MarkFirst()
        {
            CurrentLabelStatus = "First";
            Context.PrintDetails!.TotalSample += 1;
        }

        public void MarkLast()
        {
            CurrentLabelStatus = "Last";
            Context.PrintDetails!.TotalSample += 1;
        }

        public void ResetPrintType()
        {
            CurrentLabelStatus = Context.LabelInstructionDetails!.PrintType;
        }
        public void SetApproval(string userCode, string sectionId, string ipAddress)
        {
            ApprovedByUserCode = userCode;
            ApprovedBySectionId = sectionId;
            ApprovedByIpAddress = ipAddress;
            ApprovedByDateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        public void SetCurrentPair(int pairNumber, int pairCount)
        {
            CurrentPairNumber = pairNumber;
            CurrentPairCount = pairCount;
        }

        public void MoveToNextLabel()
        {
            if (Context.PrintDetails != null)
            {
                PrintedCount++;
                Context.PrintDetails.NextSequence++;

                CurrentLabelStatus = Context.LabelInstructionDetails!.PrintType;
            }
        }

        public void SetQuantity(int quantity)
        {
            TotalQuantity = quantity;
            CancellationTokenSource = new();
        }

        public void SetEndOfBatch(bool endOfBatch)
        {
            EndOfBatch = endOfBatch;
        }
        public void MarkGenerated()
        {
            CurrentStage = ProcessingStage.Generated;
        }

        public void UpdateSetNumber()
        {
            var remaining = Context.PrintDetails != null ? Context.PrintDetails.GetRemainingPrintQuantity(Context.ProductDetails?.Quantity) : throw new InvalidOperationException("RemainingQuantity is not available.");
            var setNumber = Context.PrintDetails?.SetNumber != null ? (int)Context.PrintDetails.SetNumber : throw new InvalidOperationException("SetNumber is not available.");
            Context.PrintDetails.SetNumber = remaining == 0 ? setNumber : ++setNumber;
        }

        public void MarkPrinted()
        {
            CurrentStage = ProcessingStage.Printed;
        }

        public void MarkBarcodeValidated()
        {
            CurrentStage = ProcessingStage.BarcodeValidated;
        }

        public void MarkOcrValidated()
        {
            CurrentStage = ProcessingStage.OcrValidated;
        }

        public void MarkSaved(bool hasSampleLabel)
        {
            CurrentStage = ProcessingStage.Saved;
            if (!hasSampleLabel)
            {
                Context.PrintDetails!.TotalPassed++;
            }
            Context.PrintDetails!.TotalPrinted++;

            //Update Batch No.
            if (CurrentLabelStatus == "Last")
            {
                var remaining = Context.PrintDetails != null ? Context.PrintDetails.GetRemainingPrintQuantity(Context.ProductDetails?.Quantity) : throw new InvalidOperationException("RemainingQuantity is not available.");
                var batchNumber = Context.PrintDetails?.BatchNumber != null ? (int)Context.PrintDetails.BatchNumber : throw new InvalidOperationException("BatchNumber is not available.");
                Context.PrintDetails.BatchNumber = remaining == 0 ? batchNumber : ++batchNumber;
            }

        }

        public void MarkFailed()
        {
            bool printed = CurrentStage >= ProcessingStage.Printed;

            //CurrentLabelStatus =
            //    CurrentStage >= ProcessingStage.Printed
            //        ? "Failed After Printing"
            //        : "Failed During Printing";

            Context.PrintDetails!.TotalFailed++;
            if (printed)
            {
                Context.PrintDetails.TotalPrinted++;
                Context.PrintDetails.NextSequence++;
            }

        }

        public void Reset()
        {
            var remaining = Context.PrintDetails != null ? Context.PrintDetails.GetRemainingPrintQuantity(Context.ProductDetails?.Quantity) : throw new InvalidOperationException("RemainingQuantity is not available.");
            if (remaining == 0)
            {
                Status = PrintJobStatus.Printed;
            }
            else
            {
                Status = PrintJobStatus.Ready;

            }
            PrintedCount = 0;
            CancellationTokenSource.Dispose();
        }

        public void InProgress()
        {
            Status = PrintJobStatus.InProgress;
            ResumeSignal.Set();
        }
        public void Paused()
        {
            Status = PrintJobStatus.Paused;
            ResumeSignal.Reset();
        }
        public void Stopped(bool hasApproval = false)
        {
            Status = PrintJobStatus.Stopped;
            CancellationTokenSource.Cancel();

            //var remaining = Context.PrintDetails != null ? Context.PrintDetails.GetRemainingPrintQuantity(Context.ProductDetails?.Quantity) : throw new InvalidOperationException("RemainingQuantity is not available.");
            //var setNumber = Context.PrintDetails?.SetNumber != null ? (int)Context.PrintDetails.SetNumber : throw new InvalidOperationException("SetNumber is not available.");

            //bool printed = CurrentStage >= ProcessingStage.Printed;
            //if (!hasApproval)
            //{
            //    Context.PrintDetails.SetNumber = printed ? (remaining == 0 ? setNumber : ++setNumber) : setNumber;
            //}

            // wake any paused thread
            ResumeSignal.Set();
        }
        public void Completed()
        {
            //var remaining = Context.PrintDetails != null ? Context.PrintDetails.GetRemainingPrintQuantity(Context.ProductDetails?.Quantity) : throw new InvalidOperationException("RemainingQuantity is not available.");
            //var setNumber = Context.PrintDetails?.SetNumber != null ? (int)Context.PrintDetails.SetNumber : throw new InvalidOperationException("SetNumber is not available.");
            //var batchNumber = Context.PrintDetails?.BatchNumber != null ? (int)Context.PrintDetails.BatchNumber : throw new InvalidOperationException("BatchNumber is not available.");

            //Context.PrintDetails.SetNumber = remaining == 0 ? setNumber : ++setNumber;
            //Context.PrintDetails.BatchNumber = batchNumber;

            Status = PrintJobStatus.Completed;
            CancellationTokenSource.Dispose();
        }

        public void Printed()
        {
            Status = PrintJobStatus.Printed;
        }

        //public void CheckStatus()
        //{
        //    var remaining = Context.PrintDetails != null ? Context.PrintDetails.GetRemainingPrintQuantity(Context.ProductDetails?.Quantity) : throw new InvalidOperationException("RemainingQuantity is not available.");
        //    if(remaining == 0)
        //    {
        //        Printed();
        //    }
        //}

    }
}
