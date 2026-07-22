using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Domain.Models


{
    public class KYCApplication
    {
        public Guid ApplicationId { get; set; }
        public Guid SMEId { get; set; }
        public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;
        public KycStatus Status { get; set; } = KycStatus.Pending;
        public DateTime? ReviewedAt { get; set; }

        public SME SME { get; set; } = null!;
    }
}

//Purpose:

//Tracks KYC progress.