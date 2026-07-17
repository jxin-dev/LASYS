namespace LASYS.Application.Features.BatchPrinting.Models
{
    public sealed record ApprovalAuthorizationResult(bool IsApproved, string? UserCode = null, string? SectionId = null);
}
