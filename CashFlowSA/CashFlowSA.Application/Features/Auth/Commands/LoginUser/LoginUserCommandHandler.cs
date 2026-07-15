using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using CashFlowSA.Application.Common.Settings;

namespace CashFlowSA.Application.Features.Auth.Commands.LoginUser
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginUserResult>
    {
        private readonly IApplicationDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly JwtSettings _jwtSettings;              // ← add this field

        public LoginUserCommandHandler(
            IApplicationDbContext context,
            ITokenService tokenService,
            IOptions<JwtSettings> jwtSettings)                   // ← add this parameter
        {
            _context = context;
            _tokenService = tokenService;
            _passwordHasher = new PasswordHasher<User>();
            _jwtSettings = jwtSettings.Value;                    // ← add this line
        }

        public async Task<LoginUserResult> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (user == null)
                throw new InvalidOperationException("Invalid email or password.");

            var verificationResult = _passwordHasher.VerifyHashedPassword(
                user, user.PasswordHash, request.Password);

            if (verificationResult == PasswordVerificationResult.Failed)
                throw new InvalidOperationException("Invalid email or password.");

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes);
            var session = new UserSession
            {
                SessionId = Guid.NewGuid(),
                UserId = user.UserId,
                LoginTimestamp = DateTime.UtcNow,
                RefreshToken = refreshToken,
                RefreshTokenExpiry = DateTime.UtcNow.AddDays(7),
                DeviceInformation = "Unknown",
                IPAddress = "Unknown"
            };

            _context.UserSessions.Add(session);
            await _context.SaveChangesAsync(cancellationToken);

            return new LoginUserResult
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt
            };
        }
    }
}