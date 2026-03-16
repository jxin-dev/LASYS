using System.Data;
using LASYS.Application.Interfaces.Persistence;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace LASYS.Infrastructure.Persistence.Connection
{
    public class DapperContext : IDbConnectionFactory
    {
        private readonly IConfiguration _configuration;
        private readonly DatabaseSettings _settings;

        public DapperContext(IConfiguration configuration, DatabaseSettings settings)
        {
            _configuration = configuration;
            _settings = settings;
        }

        public async Task<IDbConnection> CreateConnectionAsync()
        {
            var connectionName = _settings.Environment == "Production Environment"
                ? "ProductionConnection"
                : "TestConnection";

            var connectionString = _configuration.GetConnectionString(connectionName)
                ?? throw new InvalidOperationException("Connection string not found.");

            var connection = new MySqlConnection(connectionString);

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            return connection;
        }
    }
}
