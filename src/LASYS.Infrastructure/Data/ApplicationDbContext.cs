using MySqlConnector;
using System.Data;

namespace LASYS.Infrastructure.Data;

public class ApplicationDbContext
{
    private readonly string _connectionString;

    public ApplicationDbContext(string connectionString)
    {
        this._connectionString = connectionString;
    }
    public IDbConnection CreateConnection()
    {
        return new MySqlConnection(_connectionString);
    }

}
