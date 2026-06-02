using LASYS.Application.Interfaces.Context;

namespace LASYS.Infrastructure.Persistence.Connection
{
    public class DatabaseEnvironment : IDatabaseEnvironment
    {
        public string Current { get; set; } = "Staging";
    }
}
