using CashFlowSA.Domain.Models;

namespace CashFlowSA.Application.Common.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();

    }
}

// This interface defines the methods required for generating authentication tokens.
// GenerateAccessToken() creates a JWT for an authenticated user, while
// GenerateRefreshToken() creates a secure token used to obtain a new access token
// when the current one expires without requiring the user to log in again.