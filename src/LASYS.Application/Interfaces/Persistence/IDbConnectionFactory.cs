using System.Data;

namespace LASYS.Application.Interfaces.Persistence
{
    public interface IDbConnectionFactory
    {
        Task<IDbConnection> CreateConnectionAsync();
    }
}
