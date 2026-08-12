using LASYS.Application.Common.Enums;
using LASYS.Application.Features.Permissions.Dtos;
using LASYS.Application.Interfaces.Services;

namespace LASYS.Infrastructure.Services.Security
{
    public class PermissionService : IPermissionService
    {
        private Dictionary<string, AccessLevel> _permissions = new();

        public bool HasAccess(string functionName, AccessLevel requiredLevel)
        {
            if (!_permissions.TryGetValue(functionName, out var level))
                return false;

            return level >= requiredLevel;
        }

        public void SetPermissions(IEnumerable<PermissionDto> permissions)
        {
            _permissions = permissions.ToDictionary(
                x => x.FunctionName,
                x => x.AccessLevel);
        }

    }
}
