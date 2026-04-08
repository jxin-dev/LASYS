using LASYS.Application.Common.Enums;

namespace LASYS.Application.Features.Permissions.Dtos
{
    public class PermissionDto
    {
        public string? FunctionCode { get; init; } = default!;
        public string? FunctionName { get; init; } = default!;
        public AccessLevel AccessLevel { get; init; }
    }
}
