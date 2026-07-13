namespace CashFlowSA.Models.WalletFinancialSimulation
{
    public class Wallet
    {
        public Guid WalletId { get; set; }

        public Guid UserId { get; set; }

        public decimal Balance { get; set; } = 0;

        public string Currency { get; set; } = "ZAR";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}

//Purpose:

//One simulated wallet per SME/Investor. No real payment rail (SRS 5.6) -
//balance only ever changes via a WalletTransaction row alongside it.
