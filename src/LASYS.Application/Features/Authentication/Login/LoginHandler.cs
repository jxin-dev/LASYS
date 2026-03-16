using LASYS.Application.Common.Results;
using LASYS.Application.Interfaces.Persistence.Repositories;
using MediatR;

namespace LASYS.Application.Features.Authentication.Login
{
    public sealed class LoginHandler : IRequestHandler<LoginQuery, Result<LoginResponse>>
    {
       private readonly IUserRepository _userRepository;
        public LoginHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<LoginResponse>> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            var users = await _userRepository.GetAllUser();
            
            return Result<LoginResponse>.Success(new LoginResponse());
        }
    }
}
