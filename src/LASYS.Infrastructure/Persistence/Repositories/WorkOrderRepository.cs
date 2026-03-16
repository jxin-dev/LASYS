using LASYS.Application.Interfaces.Persistence;
using LASYS.Application.Interfaces.Persistence.Repositories;

namespace LASYS.Infrastructure.Persistence.Repositories
{
    public class WorkOrderRepository : IWorkOrderRepository
    {
        private readonly IDbConnectionFactory _factory;

        public WorkOrderRepository(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

    }
}
