using CashFlowSA.Models.enums;

namespace CashFlowSA.Models.KYCCompliance
{
    public class KYCApplication
    {
        public Guid ApplicationId { get; set; }
        public Guid UserId { get; set; }
        public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;
        public KycStatus Status { get; set; } = KycStatus.Pending;
        public DateTime? ReviewedAt { get; set; }
    }
}

//Purpose:

//Tracks KYC progress.