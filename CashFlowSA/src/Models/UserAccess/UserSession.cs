namespace CashFlowSA.Models.UserAccess
{
    public class UserSession
    {
        public Guid SessionId { get; set; }
        public Guid UserId { get; set; }
        public DateTime LoginTimestamp { get; set; } = DateTime.UtcNow;
        public DateTime? LogoutTimestamp { get; set; }
        public string DeviceInformation { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
    }
}


//Purpose:

//Security monitoring.
//Session management.
//Login auditing.
