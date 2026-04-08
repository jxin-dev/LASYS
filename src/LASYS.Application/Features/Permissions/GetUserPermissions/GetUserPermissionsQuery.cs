using LASYS.Application.Features.Permissions.Dtos;
using MediatR;

namespace LASYS.Application.Features.Permissions.GetUserPermissions
{
    public record GetUserPermissionsQuery(string RoleCode) : IRequest<List<PermissionDto>>;
}
