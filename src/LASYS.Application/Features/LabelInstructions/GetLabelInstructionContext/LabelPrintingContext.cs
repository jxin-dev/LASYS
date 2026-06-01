using LASYS.Application.Common.Models;
using LASYS.Application.Features.Products;

namespace LASYS.Application.Features.LabelInstructions.GetLabelInstructionContext
{
    public sealed record LabelPrintingContext
    {
        public LabelInstructionDetails? LabelInstructionDetails { get; init; }
        public ProductDetails? ProductDetails { get; init; }
        public MasterLabelDetails? MasterLabelDetails { get; init; }
        public PrintDetails? PrintDetails { get; init; }
    }
}
