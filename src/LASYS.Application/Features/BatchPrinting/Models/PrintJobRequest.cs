namespace LASYS.Application.Features.BatchPrinting.Models
{
    public class PrintJobRequest
    {
        public string ItemCode { get; init; } = default!;
        public int Revision { get; init; } = default!;
        public string BoxType { get; init; } = default!;
        public int Quantity { get; init; }
        public int SequencePaddingLength { get; } = 6;
    }
}
