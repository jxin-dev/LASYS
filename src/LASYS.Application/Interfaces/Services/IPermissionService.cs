using LASYS.Application.Common.Enums;
using LASYS.Application.Features.Permissions.Dtos;

namespace LASYS.Application.Interfaces.Services
{
    public interface IPermissionService
    {
        void SetPermissions(IEnumerable<PermissionDto> permissions);
        bool HasAccess(string code, AccessLevel requiredLevel);
    }
}
