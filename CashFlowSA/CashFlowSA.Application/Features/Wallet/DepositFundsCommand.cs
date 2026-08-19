using MediatR;

namespace CashFlowSA.Application.Features.Wallet.DepositFunds
{
    // Sandbox payment (SRS 5.6) -- no real payment rail. Card details are
    // never charged anywhere; they only drive the simulated approve/decline
    // outcome below. Never persist raw card numbers/CVVs in production code
    // for a real gateway -- this is fine here only because it's a sandbox.
    public class DepositFundsCommand : IRequest<DepositResultDto>
    {
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string CardNumber { get; set; } = string.Empty;
        public string ExpiryMonth { get; set; } = string.Empty;
        public string ExpiryYear { get; set; } = string.Empty;
        public string Cvv { get; set; } = string.Empty;
    }
}
