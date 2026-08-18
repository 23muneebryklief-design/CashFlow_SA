using MediatR;

namespace CashFlowSA.Application.Features.FundingRequestReview.RejectFundingRequest
{
    public class RejectFundingRequestCommand : IRequest<Unit>
    {
        public Guid FundingRequestId { get; set; }
        public Guid ReviewerId { get; set; }
        public string? Notes { get; set; }
    }
}
