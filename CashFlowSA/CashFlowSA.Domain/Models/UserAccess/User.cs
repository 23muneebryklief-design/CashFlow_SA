using CashFlowSA.Models.enums;
namespace CashFlowSA.Models.UserAccess
{
    public class User
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; }=string.Empty;
        public string LastName { get; set; }=string.Empty;
        public string Email { get; set; }=string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;       
        public AccountStatus Status { get; set; }= AccountStatus.PendingVerification;
        public UsersRoles Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }
    }
}

//Purpose:
    //Provides authentication and identity management.
    //Every SME representative, investor, analyst, admin, and auditor has a record here.
