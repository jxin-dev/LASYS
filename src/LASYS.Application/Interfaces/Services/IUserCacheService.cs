using LASYS.Domain.Security;

namespace LASYS.Application.Interfaces.Services
{
    public interface IUserCacheService
    {
        Task LoadUsersAsync(CancellationToken cancellationToken = default);
        User? GetUserByUserCode(string username);
        User? Authenticate(string username, string password);
        IReadOnlyCollection<User> GetAllUsers();
        void Clear();
    }
}
