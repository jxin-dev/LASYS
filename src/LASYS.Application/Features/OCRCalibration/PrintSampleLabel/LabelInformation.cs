using LASYS.Application.Common.Models;
using LASYS.Application.Features.Products;

namespace LASYS.Application.Features.OCRCalibration.PrintSampleLabel
{
    public sealed class LabelInformation
    {
        public LabelInstructionDetails? LabelInstructionDetails { get; init; }
        public ProductDetails? ProductDetails { get; init; }
        public MasterLabelDetails? MasterLabelDetails { get; init; }
    }
}
