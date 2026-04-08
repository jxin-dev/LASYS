using LASYS.Application.Common.Messaging;
using LASYS.Application.Common.Results;
using LASYS.Application.Interfaces.Persistence;
using LASYS.Application.Interfaces.Persistence.Repositories;
using LASYS.Application.Interfaces.Services;
using MediatR;

namespace LASYS.Application.Features.Authentication.Login
{
    public sealed class LoginHandler : IRequestHandler<LoginQuery, Result<LoginResponse>>
    {
        private readonly ILogService _logService;
        private readonly IUserRepository _userRepository;
        private readonly IImageService _imageService;
        public LoginHandler(ILogService logService, IUserRepository userRepository, IImageService imageService)
        {
            _logService = logService;
            _userRepository = userRepository;
            _imageService = imageService;
        }

        public async Task<Result<LoginResponse>> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            _logService.Log($"Login attempt for user '{request.Username}'", MessageType.Info);

            try
            {
                var user = await _userRepository.GetUserByUsernameAndPassword(request.Username, request.Password);
                if (user == null)
                {
                    _logService.Log($"Login failed for '{request.Username}'", MessageType.Warning);
                    return Result.Failure<LoginResponse>("Invalid username or password.");
                }

                await _imageService.GetUserImageUrlAsync(user.USER_CODE);

                _logService.Log($"User '{user.USER_NAME}' logged in successfully", MessageType.Info);

                return Result.Success(new LoginResponse(
                    user.USER_CODE,
                    user.USER_NAME,
                    user.SECTION_ID,
                    user.ROLE_CODE,
                    user.PLANT_CODE,
                    user.FIRST_NAME,
                    user.LAST_NAME,
                    user.MIDDLE_NAME));
            }
            catch
            {
                return Result.Failure<LoginResponse>("Unable to connect to the server. Please try again later.");
            }

        }
    }
}
