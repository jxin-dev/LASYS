using LASYS.Domain.Enums;

namespace LASYS.Domain.OCR;

public class LabelProduct
{
    // Primary Key
    public string ItemCode { get; set; } = null!;
    public string ItemName { get; set; } = null!;

    // Dimensions
    public decimal? LabelWidth { get; set; }
    public decimal? LabelHeight { get; set; }

    // Enums
    public ItemType ItemType { get; set; } = ItemType.R;
    public CalendarType CalendarType { get; set; } = CalendarType.Gregorian;
    public BarcodeType BarcodeType { get; set; } = BarcodeType.Normal;
    public BarcodeCategory BarcodeCategory { get; set; } = BarcodeCategory.TMC;
    public WithCoc WithCoc { get; set; } = WithCoc.NA;

    // Text Fields
    public string? Description { get; set; }
    public string? Description1 { get; set; }
    public string? Description2 { get; set; }

    // Flags (Mapped from char(0))
    public bool IsActive { get; set; }
    public bool HasUbLabel { get; set; }
    public bool HasCbLabel { get; set; }
    public bool HasOcbLabel { get; set; }
    public bool HasOubLabel { get; set; }
    public bool HasAubLabel { get; set; }
    public bool HasAcbLabel { get; set; }
    public bool HasCaseLabel { get; set; }
    public bool HasEuMdr { get; set; }

    // Industrial / Quantities
    public uint? UbQuantity { get; set; }
    public uint? CbQuantity { get; set; }
    public uint CbPerPallet { get; set; }
    public string? TransferInstruction { get; set; } = "PRD-EB";

    // Custom Fields
    public string? Custom1 { get; set; }
    public string? Custom2 { get; set; }
    public string? Custom3 { get; set; }
    public string? Custom4 { get; set; }
    public string? Custom5 { get; set; }

    // Metadata
    public string CreatedUserCode { get; set; } = string.Empty;
    public string CreatedSectionId { get; set; } = string.Empty;
    public string CreatedIpAddress { get; set; } = "127.0.0.1";
}