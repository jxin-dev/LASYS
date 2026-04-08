using LASYS.Application.Common.Enums;

namespace LASYS.Application.Features.Permissions.Mappings
{
    public static class PermissionMapper
    {
        public static AccessLevel MapAccessLevel(string? flag)
        {
            return flag?.ToUpperInvariant() switch
            {
                "R" => AccessLevel.Read,
                "W" => AccessLevel.ReadWrite,
                "D" => AccessLevel.Delete,
                "A" => AccessLevel.Admin,
                _ => AccessLevel.NoAccess
            };
        }
    }
}
