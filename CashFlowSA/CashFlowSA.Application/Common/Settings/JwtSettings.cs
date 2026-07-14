namespace CashFlowSA.Application.Common.Settings
{
    public class JwtSettings
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int AccessTokenExpiryMinutes { get; set; }
    }
}

// Stores the JWT configuration settings used for token generation, including
// the signing key, issuer, audience, and access token expiration time.