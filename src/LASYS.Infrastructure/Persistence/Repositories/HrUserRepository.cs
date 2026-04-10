using Dapper;
using LASYS.Application.Interfaces.Persistence;
using LASYS.Application.Interfaces.Persistence.Repositories;
using LASYS.Domain.Security;

namespace LASYS.Infrastructure.Persistence.Repositories
{
    public class HrUserRepository : IHrUserRepository
    {
        private readonly IDbConnectionFactory _factory;
        public HrUserRepository(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<HrUser?> GetEmployeeInfoByIdAsync(string employeeId)
        {

            string sql = @"SELECT USER_CODE, FIRST_NAME, MIDDLE_NAME, LAST_NAME, NICKNAME, POSITION, DEPARTMENT_CODE, SECTION_ID, SECTION_NAME, TEAM, PICTURE
                                 FROM sec_hr_users_tmp WHERE USER_CODE = @USER_CODE;";
            try
            {
                using (var connection = await _factory.CreateConnectionAsync())
                {
                    return await connection.QueryFirstOrDefaultAsync<HrUser>(sql, new { USER_CODE = employeeId });
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
