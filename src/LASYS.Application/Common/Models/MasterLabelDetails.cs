using LASYS.Application.Common.Enums;

namespace LASYS.Application.Common.Models
{
    public sealed record MasterLabelDetails
    {
        public BoxType BoxType { get; init; }
        public string ItemCode { get; init; } = string.Empty;
        public uint MasterLabelRevisionNumber { get; init; }
        public string? PlantCode { get; init; }
        public string? ItemGroupTypeCode { get; init; }
        public string? MarketCode { get; init; }
        public string? MasterLabelStatus { get; init; }
        public DateTime? MasterLabelRevisionDate { get; init; }
        public string? MasterLabelRevisionDetails { get; init; }
        public DateTime? MasterLabelEffectivityDate { get; init; }
        public string? Udi { get; init; }
        public DateTime? UdiDate { get; init; }
        public string? Roche { get; init; } = string.Empty;
        public decimal? LabelWidth { get; init; }
        public decimal? LabelHeight { get; init; }
        public byte[]? LabelFile { get; init; }
        public byte[]? ImageFile { get; init; }
        public string? FilePath { get; init; }
        public string? MaterialCode { get; init; }
        public string? MaterialDescription { get; init; }
        public decimal? MaterialConsumeQuantity { get; init; }
        public bool IsOcrSupported { get; set; }

        public MasterLabelDetails WithResolvedFilePath(string path)
        {
            return this with
            {
                FilePath = path
            };
        }

        public MasterLabelDetails WithBoxType(BoxType boxType) => this with
        {
            BoxType = boxType
        };
    }
}
