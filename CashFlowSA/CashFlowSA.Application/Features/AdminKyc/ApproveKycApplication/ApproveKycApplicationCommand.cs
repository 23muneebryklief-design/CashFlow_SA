using MediatR;

namespace CashFlowSA.Application.Features.AdminKyc.ApproveKycApplication
{
    public class ApproveKycApplicationCommand : IRequest<Unit>
    {
        public Guid ApplicationId { get; set; }
        public Guid ReviewerId { get; set; }
        public string? Notes { get; set; }
    }
}
