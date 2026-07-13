using CashFlowSA.Models.enums;

namespace CashFlowSA.Models.InvestorManagement
{
    public class Investor
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