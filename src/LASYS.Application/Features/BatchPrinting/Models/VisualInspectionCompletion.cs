using LASYS.Application.Features.BatchPrinting.Enums;

namespace LASYS.Application.Features.BatchPrinting.Models
{
    public sealed record VisualInspectionCompletion(VisualInspectionResult Result, string UserCode, string SectionId);
}
