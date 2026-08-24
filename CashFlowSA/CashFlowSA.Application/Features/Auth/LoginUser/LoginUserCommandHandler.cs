using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Domain.Models;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using CashFlowSA.Application.Common.Settings;

namespace CashFlowSA.Application.Features.Auth.LoginUser
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginUserResult>
    {
        private readonly IApplicationDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly JwtSettings _jwtSettings;

        public LoginUserCommandHandler(
            IApplicationDbContext context,
            ITokenService tokenService,
            IOptions<JwtSettings> jwtSettings)
        {
            _context = context;
            _tokenService = tokenService;
            _passwordHasher = new PasswordHasher<User>();
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<LoginUserResult> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (user == null)
                throw new AuthenticationFailedException("Invalid email or password.");

            var verificationResult = _passwordHasher.VerifyHashedPassword(
                user, user.PasswordHash, request.Password);

            if (verificationResult == PasswordVerificationResult.Failed)
                throw new AuthenticationFailedException("Invalid email or password.");

            if (user.Status == AccountStatus.Suspended)
                throw new AuthenticationFailedException("This account has been suspended. Contact an administrator.");

            if (user.Status == AccountStatus.Deactivated)
                throw new AuthenticationFailedException("This account has been deactivated.");

            // The JWT's "sub" claim is the User id, but SME/Investor-scoped
            // endpoints (e.g. KYC status, invoice listing) key off the SME/
            // Investor id instead. Resolve it once here so the frontend gets
            // it for free in the token rather than needing a follow-up call.
            Guid? profileId = user.Role switch
            {
                UsersRoles.SME => await _context.SMEs
                    .Where(s => s.UserId == user.UserId)
                    .Select(s => (Guid?)s.SMEId)
                    .FirstOrDefaultAsync(cancellationToken),
                UsersRoles.Investor => await _context.Investors
                    .Where(i => i.UserId == user.UserId)
                    .Select(i => (Guid?)i.InvestorId)
                    .FirstOrDefaultAsync(cancellationToken),
                _ => null
            };

            var accessToken = _tokenService.GenerateAccessToken(user, profileId);
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