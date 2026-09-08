using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using LASYS.Application.Interfaces.Persistence.Repositories;
using LASYS.Application.Interfaces.Services;
using LASYS.Domain.Security;

namespace LASYS.Infrastructure.Services.Session
{
    public sealed class UserCacheService : IUserCacheService
    {
        private readonly IUserRepository _userRepository;
        public UserCacheService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        private readonly ConcurrentDictionary<string, User> _users = new();
        private readonly ConcurrentDictionary<string, User> _usersByUsername = new(StringComparer.OrdinalIgnoreCase);
        public void Clear()
        {
            _users.Clear();
            _usersByUsername.Clear();
        }

        public IReadOnlyCollection<User> GetAllUsers()
        {
            return _users.Values.ToList().AsReadOnly();
        }

        public User? GetUserByUserCode(string userCode)
        {
            if (string.IsNullOrWhiteSpace(userCode))
                return null;

            _users.TryGetValue(userCode, out var user);

            return user;
        }

        public async Task LoadUsersAsync(CancellationToken cancellationToken = default)
        {
            var users = await _userRepository.GetAllUsersAsync();

            _users.Clear();
            _usersByUsername.Clear();


            foreach (var user in users)
            {
                if (!string.IsNullOrWhiteSpace(user.USER_CODE))
                {
                    _users[user.USER_CODE] = user;
                }

                if (!string.IsNullOrWhiteSpace(user.USER_NAME))
                {
                    _usersByUsername[user.USER_NAME.Trim()] = user;
                }
            }
        }

        public User? Authenticate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) ||
               string.IsNullOrEmpty(password))
            {
                return null;
            }

            if (!_usersByUsername.TryGetValue(
                username.Trim(),
                out var user))
            {
                return null;
            }

            var passwordHash = MD5.HashData(
                Encoding.UTF8.GetBytes(password));

            var hash = Convert.ToHexString(passwordHash)
                .ToLowerInvariant();

            return user.USER_PASSWORD == hash
                ? user
                : null;
        }
    }
}
