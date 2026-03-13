using LASYS.Application.Interfaces;

namespace LASYS.Infrastructure.Repositories
{
    public class WorkOrderRepository : IWorkOrderRepository
    {
        public Task<string?> GetTemplatePathAsync(int workOrderId)
        {
            //return Task.FromResult<string?>("test"); //Temporary data
            return Task.FromResult<string?>(null); //Temporary data
        }
    }
}
