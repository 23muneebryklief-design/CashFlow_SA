using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CashFlowSA.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace CashFlowSA.Infrastructure.Services
{
    public sealed class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool IsAuthenticated =>
            _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

        public Guid? UserId
        {
            get
            {
                var principal = _httpContextAccessor.HttpContext?.User;
                if (principal?.Identity?.IsAuthenticated != true)
                    return null;

                var raw = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                          ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? principal.FindFirstValue("userId");

                return Guid.TryParse(raw, out var userId) ? userId : null;
            }
        }

        public string? IpAddress => _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
    }
}
