using LASYS.Application.Common.Results;
using LASYS.Application.Features.Authentication.Login;
using MediatR;

namespace LASYS.Application.Features.Authentication.AutoLogin
{
    public sealed record AutoLoginCommand(string Username) : IRequest<Result<LoginResponse>>;
}
