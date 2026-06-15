using System.Data;
using LASYS.Application.Common.Messaging;
using LASYS.Application.Interfaces.Context;
using LASYS.Application.Interfaces.Persistence;
using LASYS.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace LASYS.Infrastructure.Persistence.Connection
{
    public class DapperContext : IDbConnectionFactory
    {
        private readonly IConfiguration _configuration;
        private readonly IDatabaseEnvironment _databaseEnvironment;
        private readonly ILogService _logService;

        public DapperContext(IConfiguration configuration, IDatabaseEnvironment databaseEnvironment, ILogService logService)
        {
            _configuration = configuration;
            _databaseEnvironment = databaseEnvironment;
            _logService = logService;
        }


        public async Task<IDbConnection> CreateConnectionAsync()
        {
            var environment = _databaseEnvironment.Current;

            var connectionString =
                _configuration.GetConnectionString(environment);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                _logService.Log(
                    $"Connection string not found for environment: {environment}",
                    MessageType.Error);

                throw new InvalidOperationException(
                    $"Connection string '{environment}' not found.");
            }

            var connection = new MySqlConnection(connectionString);

            try
            {
                await connection.OpenAsync();

                _logService.Log(
                    $"Database connection opened ({environment})",
                    MessageType.Info);

                return connection;
            }
            catch (Exception ex)
            {
                _logService.Log(
                    $"Database connection failed ({environment}) - {ex.Message}",
                    MessageType.Error);

                connection.Dispose();
                throw;
            }
        }
    }
}
