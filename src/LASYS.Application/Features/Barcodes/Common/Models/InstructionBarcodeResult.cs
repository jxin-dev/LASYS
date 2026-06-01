namespace LASYS.Application.Features.Barcodes.Common.Models
{
    public sealed class InstructionBarcodeResult
    {
        public string? InstructionBarcodeScanned { get; set; }
        public InstructionType InstructionType { get; set; } = InstructionType.NotSet;
        public PackagingType PackagingType { get; set; } = PackagingType.NotSet;
        public InstructionStatus InstructionStatus { get; set; } = InstructionStatus.NotSet;
        public string? InstructionProductBarcodeNumber { get; set; }
        public int? InstructionRevision { get; set; }
        public string? InstructionLotNumber { get; set; }
        public string? InstructionLineCode { get; set; }
    }
}
