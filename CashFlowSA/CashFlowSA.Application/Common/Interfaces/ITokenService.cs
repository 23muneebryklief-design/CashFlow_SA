using CashFlowSA.Domain.Models;

namespace CashFlowSA.Application.Common.Interfaces
{
    public interface ITokenService
    {
        // profileId is the SME/Investor id (not the User id) -- included as a
        // "profileId" claim so the frontend can call SME/Investor-scoped
        // endpoints (e.g. KYC status) right after login without an extra
        // lookup round-trip. Null for roles with no such profile (Admin, etc).
        string GenerateAccessToken(User user, Guid? profileId = null);
        string GenerateRefreshToken();

    }
}

// This interface defines the methods required for generating authentication tokens.
// GenerateAccessToken() creates a JWT for an authenticated user, while
// GenerateRefreshToken() creates a secure token used to obtain a new access token
// when the current one expires without requiring the user to log in again.