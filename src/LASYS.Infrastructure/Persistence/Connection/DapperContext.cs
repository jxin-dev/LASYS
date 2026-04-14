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
        private readonly IConfiguration _configuration;
        private readonly DatabaseSettings _settings;
        private readonly ILogService _logService;

        public DapperContext(IConfiguration configuration, DatabaseSettings settings, ILogService logService)
        {
            _configuration = configuration;
            _settings = settings;
            _logService = logService;
        }

        public async Task<IDbConnection> CreateConnectionAsync()
        {
            _logService.Log($"Environment: {_settings.Environment}", MessageType.Info);

            var env = _settings.Environment ?? "Production";

            var connectionString = _configuration.GetConnectionString(env)
                ?? throw new InvalidOperationException("Connection string not found.");

            var connection = new MySqlConnection(connectionString);

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    _logService.Log($"Opening database connection (Environment: {_settings.Environment})", MessageType.Info);
                    await connection.OpenAsync();
                }
            }
            catch (Exception ex)
            {
                _logService.Log($"Database connection failed (Environment: {_settings.Environment}) - {ex.Message}", MessageType.Error);
                throw;
            }

            return connection;
        }
    }
}
