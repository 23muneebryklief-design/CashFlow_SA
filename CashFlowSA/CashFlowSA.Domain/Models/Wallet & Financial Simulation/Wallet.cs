namespace CashFlowSA.Domain.Models
{
    public class Wallet : BaseEntity
    {
        public Guid WalletId { get; set; }

        public Guid UserId { get; set; }

        public decimal Balance { get; set; } = 0;

        public string Currency { get; set; } = "ZAR";
    }
}

//Purpose:

//One simulated wallet per SME/Investor. No real payment rail (SRS 5.6) -
//balance only ever changes via a WalletTransaction row alongside it.
