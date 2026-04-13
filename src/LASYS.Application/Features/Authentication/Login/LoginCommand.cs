using LASYS.Application.Common.Results;
using MediatR;

namespace LASYS.Application.Features.Authentication.Login
{
    public sealed record LoginCommand(string Username, string Password) : IRequest<Result<LoginResponse>>;
}
