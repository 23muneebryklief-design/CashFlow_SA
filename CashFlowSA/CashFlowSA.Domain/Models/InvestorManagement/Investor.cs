using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Domain.Models
{
    public class Investor : BaseEntity
    {
        public Guid InvestorId { get; set; }

        public Guid UserId { get; set; }

        public string Address { get; set; } = string.Empty;

        public RiskAppetite RiskAppetite { get; set; } = RiskAppetite.Low;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}

//Purpose:

//Represents funding providers.