namespace LASYS.Application.Features.NiceLabels
{
    public sealed record NiceLabel
    {
        public string? NiceLabelPath { get; init; }
        public byte[]? NiceLabelFile { get; init; }
    }
}
