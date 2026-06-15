using LASYS.Application.Interfaces.Context;
using Microsoft.Extensions.Options;

namespace LASYS.Infrastructure.Persistence.Connection
{
    public class DatabaseEnvironment : IDatabaseEnvironment
    {
        public string Current { get; set; }
        public DatabaseEnvironment(IOptions<DatabaseSettings> options)
        {
            Current = options.Value.Environment;
        }
    }
}
