using LASYS.Domain.Security;

namespace LASYS.Application.Interfaces.Persistence.Repositories
{
    public interface IHrUserRepository
    {
        Task<HrUser?> GetEmployeeInfoByIdAsync(string employeeId);
    }
}
