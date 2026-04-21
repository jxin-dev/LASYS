using System.Data;
using LASYS.Application.Common.Messaging;
using LASYS.Application.Interfaces.Persistence;
using LASYS.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace LASYS.Infrastructure.Persistence.Connection
{
    public class DapperContext : IDbConnectionFactory
    {
        private readonly ILogService _logService;
        private readonly string _connectionString;

        public DapperContext(IConfiguration configuration, DatabaseSettings settings, ILogService logService)
        {
            _logService = logService;

            var env = settings.Environment ?? "Production";
            _connectionString = configuration.GetConnectionString(env)
                ?? throw new InvalidOperationException("Connection string not found.");
        }

        public async Task<IDbConnection> CreateConnectionAsync()
        {
            var connection = new MySqlConnection(_connectionString);

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }
            }
            catch (Exception ex)
            {
                _logService.Log($"Database connection failed - {ex.Message}", MessageType.Error);
                throw;
            }

            return connection;
        }
    }
}
