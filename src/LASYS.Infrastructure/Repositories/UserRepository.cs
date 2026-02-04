using Dapper;
using LASYS.Application.Interfaces;
using LASYS.Domain.Security;
using LASYS.Infrastructure.Data;

namespace LASYS.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository()
        {
            _context = new ApplicationDbContext("Server=127.0.0.1;Port=3306;Database=otpclasysdb;Uid=root;Pwd=Passw0rd;");
        }

        public async Task CreateUser(User user)
        {
            string sql = @"
            INSERT INTO sec_users_mst 
            (
                USER_CODE, USER_NAME, USER_PASSWORD, FIRST_NAME, MIDDLE_NAME, LAST_NAME,
                SECTION_ID, ROLE_CODE, PLANT_CODE, COMMENT, ACCESS_FLAG, ACTIVE_FLAG,
                CREATED_USER_CODE, CREATED_SECTION_ID, CREATED_IP_ADDRESS, CREATED_DATETIME,
                LASTUPDATE_USER_CODE, LASTUPDATE_SECTION_ID, LASTUPDATE_IP_ADDRESS,
                LASTUPDATE_DATETIME, LASTPASSRENEW_DATETIME
            )
            VALUES 
            (
                @USER_CODE, @USER_NAME, MD5(@USER_PASSWORD), @FIRST_NAME, @MIDDLE_NAME, @LAST_NAME,
                @SECTION_ID, @ROLE_CODE, @PLANT_CODE, @COMMENT, @ACCESS_FLAG, @ACTIVE_FLAG,
                @CREATED_USER_CODE, @CREATED_SECTION_ID, INET_ATON(@CREATED_IP_ADDRESS), CURRENT_TIMESTAMP+0,
                @LASTUPDATE_USER_CODE, @LASTUPDATE_SECTION_ID, INET_ATON(@LASTUPDATE_IP_ADDRESS),
                CURRENT_TIMESTAMP+0, CURRENT_TIMESTAMP+0
            );";


            try
            {
                using (var connection = _context.CreateConnection())
                {
                    await connection.ExecuteAsync(sql, user);
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task DeleteUser(string code)
        {
            string sql = "DELETE FROM sec_users_mst WHERE USER_CODE = @USER_CODE;";

            try
            {
                using (var connection = _context.CreateConnection())
                {
                    await connection.ExecuteAsync(sql, code);
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetAllUser()
        {
            string sql = "select * from sec_users_mst";

            try
            {
                using (var connection = _context.CreateConnection())
                {
                   return await connection.QueryAsync<User>(sql);
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<User> GetUserByCode(string code)
        {
            string sql = "select * from sec_users_mst where USER_CODE = @USER_CODE";

            try
            {
                using (var connection = _context.CreateConnection())
                {
                    return await connection.QueryFirstAsync<User>(sql, new { USER_CODE = code });
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<User> GetUserByUsername(string username)
        {
            string sql = "select * from sec_users_mst where USER_NAME = @USER_NAME";

            try
            {
                using (var connection = _context.CreateConnection())
                {
                    return await connection.QueryFirstAsync<User>(sql, new { USER_NAME = username });
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<User> GetUserByUsernameAndPassword(string username, string password)
        {
            string sql = @"
                SELECT USR.USER_CODE, USR.USER_NAME, USR.FIRST_NAME, USR.MIDDLE_NAME, USR.LAST_NAME, USR.ROLE_CODE, USR.SECTION_ID, 
                SEC.NAME AS SECTION_NAME, USR.LASTPASSRENEW_DATETIME, IF(ISNULL(USR.ACTIVE_FLAG), False, True) AS 'ACTIVE_FLAG' 
                FROM sec_users_mst USR LEFT JOIN sec_sections_mst SEC ON SEC.ID = USR.SECTION_ID
                WHERE USR.USER_NAME = @USER_NAME AND USR.USER_PASSWORD = MD5(@USER_PASSWORD)
                AND USR.ACTIVE_FLAG = ' ' LIMIT 1";
            try
            {
                using (var connection = _context.CreateConnection())
                {
                    return await connection.QueryFirstAsync<User>(sql, new { USER_NAME = username, USER_PASSWORD = password });
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task UpdateUser(User user)
        {
            string sql = @"
            UPDATE sec_users_mst
            SET 
                USER_NAME = @USER_NAME,
                USER_PASSWORD = MD5(@USER_PASSWORD),
                FIRST_NAME = @FIRST_NAME,
                MIDDLE_NAME = @MIDDLE_NAME,
                LAST_NAME = @LAST_NAME,
                SECTION_ID = @SECTION_ID,
                ROLE_CODE = @ROLE_CODE,
                PLANT_CODE = @PLANT_CODE,
                COMMENT = @COMMENT,
                ACCESS_FLAG = @ACCESS_FLAG,
                ACTIVE_FLAG = @ACTIVE_FLAG,
                LASTUPDATE_USER_CODE = @LASTUPDATE_USER_CODE,
                LASTUPDATE_SECTION_ID = @LASTUPDATE_SECTION_ID,
                LASTUPDATE_IP_ADDRESS = INET_ATON(@LASTUPDATE_IP_ADDRESS),
                LASTUPDATE_DATETIME = CURRENT_TIMESTAMP+0
            WHERE USER_CODE = @USER_CODE;";

            try
            {
                using (var connection = _context.CreateConnection())
                {
                    await connection.QueryAsync(sql, user);
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
