using LASYS.Domain.Security;

namespace LASYS.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetUserByUsernameAndPassword(string username, string password);
        Task<User> GetUserByCode(string code);
        Task<User> GetUserByUsername(string username);
        Task<IEnumerable<User>> GetAllUser();
        Task CreateUser(User user);
        Task UpdateUser(User user);
        Task DeleteUser(string code);
    }
}
