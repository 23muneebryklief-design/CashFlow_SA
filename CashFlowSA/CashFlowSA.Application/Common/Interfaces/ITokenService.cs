using CashFlowSA.Domain.Models;

namespace CashFlowSA.Application.Common.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
    }
}