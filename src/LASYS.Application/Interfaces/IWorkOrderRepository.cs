namespace LASYS.Application.Interfaces
{
    public interface IWorkOrderRepository
    {
        Task<string?> GetTemplatePathAsync(int workOrderId);
    }
}
