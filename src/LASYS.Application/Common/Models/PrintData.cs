using LASYS.Application.Common.Enums;
using LASYS.Domain.Instruction;
using System.Data;

namespace LASYS.Application.Common.Models
{
    public class PrintData
    {
        // Identity
        public string ItemCode { get; set; } = string.Empty;
        public string LotNo { get; set; } = string.Empty;
        public string InstructionCode { get; set; } = string.Empty;

        // Print configuration
        public BoxType LabelType { get; set; } = BoxType.NotSet;
        public LabelPrintType LabelStatus { get; set; } = LabelPrintType.NotSet;
        public int PrintLabelPrintType { get; set; }
        public bool IsPaired { get; set; }
        public bool IsQcSample { get; set; }

        // Quantities / sequence
        public long ProductQuantity { get; set; }
        public long TotalCase { get; set; }
        public long TotalPassedCase { get; set; }
        public long TotalFailedCase { get; set; }
        public int TotalSampleCase { get; set; }
        public long LastSequence { get; set; }
        public long TargetQuantity { get; set; }
        public long TotalPassed { get; set; }
        public long SequenceNumber { get; set; }

        // Dates
        public DateTime ExpiryDate { get; set; } = DateTime.MinValue;
        public DateTime ProductionDate { get; set; } = DateTime.MinValue;
        public string MfgDate { get; set; } = string.Empty;

        // Miscellaneous
        public string PssNumber { get; set; } = string.Empty;
        public int RevisionNumber { get; set; }
        public string ProductBarcode { get; set; } = string.Empty;
        public string ApprovedUserCode { get; set; } = string.Empty;
        public int ApprovedSectionId { get; set; }

        // Result rows to be saved after printing
        public InstructionPrintSnapshot? ResultData { get; set; }
    }
}
