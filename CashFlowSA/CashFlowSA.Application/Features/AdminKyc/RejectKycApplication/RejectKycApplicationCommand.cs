using MediatR;

namespace CashFlowSA.Application.Features.AdminKyc.RejectKycApplication
{
    public class RejectKycApplicationCommand : IRequest<Unit>
    {
        public Guid ApplicationId { get; set; }
        public Guid ReviewerId { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
