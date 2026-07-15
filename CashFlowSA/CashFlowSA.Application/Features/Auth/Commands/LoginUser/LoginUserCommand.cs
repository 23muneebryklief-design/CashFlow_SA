using MediatR;

namespace CashFlowSA.Application.Features.Auth.Commands.LoginUser
{
    public class LoginUserCommand : IRequest<LoginUserResult>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginUserResult
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}