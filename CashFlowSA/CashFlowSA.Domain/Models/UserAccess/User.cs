using CashFlowSA.Domain.Models.Enums;
namespace CashFlowSA.Domain.Models
{
    public class User : BaseEntity
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; }=string.Empty;
        public string LastName { get; set; }=string.Empty;
        public string Email { get; set; }=string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;       
        public AccountStatus Status { get; set; }= AccountStatus.PendingVerification;
        public UsersRoles Role { get; set; }
        public DateTime? LastLogin { get; set; }
        

        public ICollection<UserSession> UserSessions { get; set; }
        = new List<UserSession>();
        public ICollection<SME> SMEs { get; set; }
        = new List<SME>();
    }
}

//Purpose:
    //Provides authentication and identity management.
    //Every SME representative, investor, analyst, admin, and auditor has a record here.
