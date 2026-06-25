using LASYS.Application.Common.Enums;

namespace LASYS.Application.Features.OCRCalibration.GetOcrSupportedItems
{
    public sealed record OcrSupportedItemDto
    {
        public string ItemCode { get; init; } = string.Empty;
        public uint LabelInstructionRevNumber { get; init; }
        public uint MasterLabelRevNumber { get; init; }
        public IReadOnlyCollection<BoxType>? AvailableBoxTypes { get; init; } = [];
    };
}
