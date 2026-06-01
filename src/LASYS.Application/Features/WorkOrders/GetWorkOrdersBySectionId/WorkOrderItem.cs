using LASYS.Application.Common.Enums;

namespace LASYS.Application.Features.WorkOrders.GetWorkOrdersBySectionId
{
    public sealed record Product
    {
        public string AvailableBoxTypes { get; init; } = string.Empty;
        public LineSectionAssignment LineSectionAssignment { get; init; } = new();
        public string ItemCode { get; init; } = string.Empty;
        public string LotNo { get; init; } = string.Empty;
        public uint LabelInsRevNumber { get; init; }
        public uint MasterLabelRevNumber { get; init; }
        public string Udi { get; init; } = string.Empty;
        public bool IsEumdr { get; init; }
        public DateTime? ManufactureDate { get; init; }
        public DateTime? ExpirationDate { get; init; }
        public DateTime? ProductionDate { get; init; }
        public DateTime? SterilizationDate { get; init; }
        public int TargetProductionQuantity { get; init; }

        
        public LabelDetails? UbLabel { get; init; }
        public LabelDetails? AubLabel { get; init; }
        public LabelDetails? OubLabel { get; init; }
        public LabelDetails? CbLabel { get; init; }
        public LabelDetails? AcbLabel { get; init; }
        public LabelDetails? OcbLabel { get; init; }

        public LabelDetails? GetLabelDetails(BoxType boxType)
        {
            return boxType switch
            {
                BoxType.UnitBox => UbLabel,
                BoxType.AdditionalUnitBox => AubLabel,
                BoxType.OuterUnitBox => OubLabel,
                BoxType.CartonBox => CbLabel,
                BoxType.AdditionalCartonBox => AcbLabel,
                BoxType.OuterCartonBox => OcbLabel,
                _ => null
            };
        }

        public Product WithLabelDetails(BoxType boxType, LabelDetails updated)
        {
            return boxType switch
            {
                BoxType.UnitBox => this with { UbLabel = updated },
                BoxType.AdditionalUnitBox => this with { AubLabel = updated },
                BoxType.OuterUnitBox => this with { OubLabel = updated },
                BoxType.CartonBox => this with { CbLabel = updated },
                BoxType.AdditionalCartonBox => this with { AcbLabel = updated },
                BoxType.OuterCartonBox => this with { OcbLabel = updated },
                _ => this
            };
        }

    }

    public sealed record LineSectionAssignment
    {
        public string LineCode { get; init; } = string.Empty;
        public IReadOnlyList<int> SectionIds { get; init; } = [];

        public LineSectionAssignment WithSectionIds(string sectionIds) => this with
        {
            SectionIds = [.. sectionIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(int.Parse)]
        };
    }


    public sealed record LabelDetails
    {
        public string? InstructionCode { get; private set; }
        public uint? TargetPrintQuantity { get; private set; }
        public string? PrintType { get; private set; }
        public string? Verdict { get; private set; }
        public string? InstructionStatus { get; private set; }
        public string? LabelStatus { get; private set; }
        public string? ApprovedBy { get; private set; }
        public DateTime? DateApproved { get; private set; }
        public string? ReportType { get; private set; }
        public string? BarcodeNumber { get; private set; }
        public uint? Quantity { get; private set; } //ex. UB_QUANTITY, CB_QUANTITY etc.
        public LabelDetails(
            string? instructionCode,
            uint? targetPrintQuantity,
            string? printType,
            string? verdict,
            string? instructionStatus,
            string? labelStatus,
            string? approvedBy,
            DateTime? dateApproved,
            string? reportType,
            string? barcodeNumber,
            uint? quantity)
        {
            InstructionCode = instructionCode;
            TargetPrintQuantity = targetPrintQuantity;
            PrintType = printType;
            Verdict = verdict;
            InstructionStatus = instructionStatus;
            LabelStatus = labelStatus;
            ApprovedBy = approvedBy;
            DateApproved = dateApproved;
            ReportType = reportType;
            BarcodeNumber = barcodeNumber;
            Quantity = quantity;
        }
        public NiceLabelDetails? NiceLabelDetails { get; init; }
        public PrintDetails? PrintDetails { get; init; }

        public LabelDetails WithNiceLabelDetails(string niceLabelTemplate) => this with
        {
            NiceLabelDetails = new NiceLabelDetails(niceLabelTemplate).WithLoadingStatus(true)
        };
        public LabelDetails WithPrintDetails(PrintDetails printDetails) => this with
        {
            PrintDetails = printDetails
        };
    }

    public sealed record NiceLabelDetails
    {
        public bool IsLoaded { get; private set; }
        public string? NiceLabelPath { get; private set; }
        public byte[]? NiceLabelFile { get; init; }
        public NiceLabelDetails(string niceLabelPath)
        {
            NiceLabelPath = niceLabelPath;
        }
        public NiceLabelDetails WithLoadingStatus(bool isLoaded) => this with
        {
            IsLoaded = isLoaded
        };
        public NiceLabelDetails WithNiceLabelFile(byte[]? niceLabelFile) => this with
        {
            NiceLabelFile = niceLabelFile
        };
    }

    public sealed record PrintDetails
    {
        public long TotalPassed { get; init; }
        public long TotalFailed { get; init; }
        public long TotalSampled { get; init; }
        public long TotalPrinted { get; init; }
        public long NextSequenceNumber { get; init; }
        public long BatchNumber { get; init; }
        public long SetNumber { get; init; }
        public long GetRemainingPrintQuantity(uint? targetPrintQuantity) => (long)(targetPrintQuantity ?? 0) - TotalPrinted;
        
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
        public PrintDetails WithNextSequenceNumber(long nextSequenceNumber) => this with
        {
            NextSequenceNumber = nextSequenceNumber
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


    public record class WorkOrderItem(
        string? AvailableBoxTypes,
        string SectionAssignments,
        string ItemCode,
        string LotNo,
        uint LabelInsRevNumber,
        uint MasterLabelRevNumber,
        string Udi,
        DateTime ExpirationDate,
        int TargetProductionQuantity,
        bool IsEumdr,
        DateTime? ManufactureDate,
        BoxLabelDto? UnitBox,
        BoxLabelDto? AdditionalUnitBox,
        BoxLabelDto? OuterUnitBox,
        BoxLabelDto? CartonBox,
        BoxLabelDto? AdditionalCartonBox,
        BoxLabelDto? OuterCartonBox)
    {

        public WorkOrderItem WithBoxLabel(BoxType boxType, BoxLabelDto? boxLabel)
        {
            return boxType switch
            {
                BoxType.UnitBox => this with { UnitBox = boxLabel },
                BoxType.AdditionalUnitBox => this with { AdditionalUnitBox = boxLabel },
                BoxType.OuterUnitBox => this with { OuterUnitBox = boxLabel },
                BoxType.CartonBox => this with { CartonBox = boxLabel },
                BoxType.AdditionalCartonBox => this with { AdditionalCartonBox = boxLabel },
                BoxType.OuterCartonBox => this with { OuterCartonBox = boxLabel },
                _ => this
            };
        }

        public BoxLabelDto? GetBoxLabel(BoxType boxType)
        {
            return boxType switch
            {
                BoxType.UnitBox => UnitBox,
                BoxType.AdditionalUnitBox => AdditionalUnitBox,
                BoxType.OuterUnitBox => OuterUnitBox,
                BoxType.CartonBox => CartonBox,
                BoxType.AdditionalCartonBox => AdditionalCartonBox,
                BoxType.OuterCartonBox => OuterCartonBox,
                _ => null
            };
        }
    };

    public record class BoxLabelDto
    {
        public string? InstructionCode { get; init; }
        public uint? TargetPrintQuantity { get; init; }
        public string? PrintType { get; init; }
        public string? Verdict { get; init; }
        public string? InstructionStatus { get; init; }
        public string? LabelStatus { get; init; }
        public string? ApprovedBy { get; init; }
        public DateTime? DateApproved { get; init; }

        // Box type details
        public string? ReportType { get; init; }
        public string? BarcodeNumber { get; init; }
        public uint? Quantity { get; init; } //ex. UB_QUANTITY, CB_QUANTITY etc.
        //

        public bool? IsNiceLabelLoaded { get; init; }
        public string? NiceLabelPath { get; init; }
        public byte[]? NiceLabelFile { get; init; }

        public long? TotalPassed { get; init; }
        public long? TotalFailed { get; init; }
        public long? TotalSampled { get; init; }
        public long? TotalPrinted { get; init; }

        public long? NextSequenceNumber { get; init; }
        public long? BatchNumber { get; init; }
        public long? SetNumber { get; init; }

        public long RemainingPrintQuantity =>
            (long)(TargetPrintQuantity ?? 0) - (TotalPrinted ?? 0);

        public BoxLabelDto WithPrintDetails(
            long totalPassed,
            long totalFailed,
            long totalSampled,
            long totalPrinted,
            long nextSequenceNumber,
            long batchNumber,
            long setNumber)
        {
            return this with
            {
                TotalPassed = totalPassed,
                TotalFailed = totalFailed,
                TotalSampled = totalSampled,
                TotalPrinted = totalPrinted,
                NextSequenceNumber = nextSequenceNumber,
                BatchNumber = batchNumber,
                SetNumber = setNumber
            };
        }

        public BoxLabelDto WithNiceLabelFile(byte[]? niceLabelFile)
        {
            return this with
            {
                NiceLabelFile = niceLabelFile
            };
        }
        public BoxLabelDto WithNiceLabelPath(string? niceLabelPath)
        {
            return this with
            {
                NiceLabelPath = niceLabelPath
            };
        }
        public BoxLabelDto WithNiceLabelLoadingStatus(bool isLoaded)
        {
            return this with
            {
                IsNiceLabelLoaded = isLoaded
            };
        }
    }
}
