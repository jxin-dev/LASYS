using Dapper;
using LASYS.Application.Features.Permissions.Dtos;
using LASYS.Application.Features.Permissions.Mappings;
using LASYS.Application.Interfaces.Persistence;
using LASYS.Domain.Security;
using MediatR;

namespace LASYS.Application.Features.Permissions.GetUserPermissions
{
    public class GetUserPermissionsHandler : IRequestHandler<GetUserPermissionsQuery, List<PermissionDto>>
    {
        private readonly IDbConnectionFactory _factory;

        public GetUserPermissionsHandler(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<List<PermissionDto>> Handle(GetUserPermissionsQuery request, CancellationToken cancellationToken)
        {
            string sql = @"SELECT
                                MST.SYSTEM_FUNCTION_CODE,
                                MST.SYSTEM_FUNCTION_NAME,
                                MST.SYSTEM_PROCESS_CODE,
                                MST.SYSTEM_PROCESS_NAME,
                                COALESCE(TCL.ACCESS_FLAG, 'N') AS ACCESS_FLAG,
                                COALESCE(TCL.ACTION_ROLES_ID, 0) AS ACTION_ROLES_ID
                            FROM
                                sec_system_functions_mst AS MST
                                    LEFT JOIN
                                sec_action_roles_tcl AS TCL ON MST.SYSTEM_FUNCTION_CODE = TCL.SYSTEM_FUNCTION_CODE
                                    AND ROLE_CODE = @ROLE_CODE
                            WHERE
                                MST.ACTIVE_FLAG = ' '
                            ORDER BY MST.SYSTEM_PROCESS_NAME;";


            try
            {
                using (var connection = await _factory.CreateConnectionAsync())
                {
                    var result = await connection.QueryAsync<Permission>(sql, new { ROLE_CODE = request.RoleCode });
                    return result.Select(x => new PermissionDto
                    {
                        FunctionCode = x.SYSTEM_FUNCTION_CODE,
                        FunctionName = x.SYSTEM_FUNCTION_NAME,
                        AccessLevel = PermissionMapper.MapAccessLevel(x.ACCESS_FLAG)
                    }).ToList();
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
