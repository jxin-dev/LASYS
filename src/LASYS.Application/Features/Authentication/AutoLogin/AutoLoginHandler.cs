using LASYS.Application.Common.Messaging;
using LASYS.Application.Common.Results;
using LASYS.Application.Features.Authentication.Login;
using LASYS.Application.Features.Permissions.GetUserPermissions;
using LASYS.Application.Interfaces.Persistence.Repositories;
using LASYS.Application.Interfaces.Services;
using MediatR;

namespace LASYS.Application.Features.Authentication.AutoLogin
{
    public sealed class AutoLoginHandler : IRequestHandler<AutoLoginCommand, Result<LoginResponse>>
    {
        private readonly ILogService _logService;
        private readonly IUserRepository _userRepository;
        private readonly IHrUserRepository _hrUserRepository;
        private readonly IImageService _imageService;
        private readonly IPermissionService _permissionService;
        private readonly IMediator _mediator;

        public AutoLoginHandler(ILogService logService,
                                IUserRepository userRepository,
                                IHrUserRepository hrUserRepository,
                                IImageService imageService,
                                IPermissionService permissionService,
                                IMediator mediator)
        {
            _logService = logService;
            _userRepository = userRepository;
            _hrUserRepository = hrUserRepository;
            _imageService = imageService;
            _permissionService = permissionService;
            _mediator = mediator;
        }

        public async Task<Result<LoginResponse>> Handle(AutoLoginCommand request, CancellationToken cancellationToken)
        {
            _logService.Log($"Login attempt for user '{request.Username}'", MessageType.Info);

            try
            {
                var user = await _userRepository.GetUserByUsername(request.Username);
                if (user == null)
                {
                    _logService.Log($"Login failed for '{request.Username}'", MessageType.Warning);
                    return Result.Failure<LoginResponse>("Invalid username or password.");
                }

                var hrUser = await _hrUserRepository.GetEmployeeInfoByIdAsync(user.USER_CODE);
                if (hrUser == null)
                {
                    _logService.Log($"HR record not found for '{request.Username}'", MessageType.Warning);
                    return Result.Failure<LoginResponse>("Employee information not found.");
                }

                var imagePath = await _imageService.GetUserImageUrlAsync(user.USER_CODE);

                var permissions = await _mediator.Send(new GetUserPermissionsQuery(user.ROLE_CODE!), cancellationToken);
                _permissionService.SetPermissions(permissions);


                _logService.Log($"User '{user.USER_NAME}' logged in successfully", MessageType.Info);

                return Result.Success(new LoginResponse(
                    user.USER_CODE,
                    user.USER_NAME,
                    user.SECTION_ID,
                    user.ROLE_CODE,
                    user.PLANT_CODE,
                    user.FIRST_NAME,
                    user.LAST_NAME,
                    user.MIDDLE_NAME,
                    hrUser.NICKNAME,
                    hrUser.POSITION,
                    hrUser.DEPARTMENT_CODE,
                    hrUser.SECTION_NAME,
                    imagePath));
            }
            catch
            {
                return Result.Failure<LoginResponse>("Unable to connect to the server. Please try again later.");
            }
        }
    }
}
