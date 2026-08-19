using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.FundingRequestReview.RejectFundingRequest
{
    public class RejectFundingRequestCommandHandler : IRequestHandler<RejectFundingRequestCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public RejectFundingRequestCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(RejectFundingRequestCommand request, CancellationToken cancellationToken)
        {
            var fundingRequest = await _context.FundingRequests
                .FirstOrDefaultAsync(r => r.FundingRequestId == request.FundingRequestId, cancellationToken);

            if (fundingRequest is null)
                throw new NotFoundException("Funding request not found.");

            if (fundingRequest.Status != FundingRequestStatus.Pending
                && fundingRequest.Status != FundingRequestStatus.UnderReview)
                throw new ConflictException("Only Pending or UnderReview funding requests can be rejected.");

            fundingRequest.Status = FundingRequestStatus.Rejected;
            fundingRequest.DecisionAt = DateTime.UtcNow;
            fundingRequest.ReviewerId = request.ReviewerId;
            fundingRequest.ReviewNotes = request.Notes;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
