using MediatR;

namespace CashFlowSA.Application.Features.Wallet.WithdrawFunds
{
    // Sandbox payout (SRS 5.6) -- no real payment rail. Bank details are
    // never actually paid out anywhere; they only drive the simulated
    // approve/decline outcome below, same convention as DepositFunds' card
    // details. Never persist raw account numbers in production code for a
    // real payout rail -- this is fine here only because it's a sandbox.
    public class WithdrawFundsCommand : IRequest<WithdrawResultDto>
    {
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string AccountHolderName { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }
}