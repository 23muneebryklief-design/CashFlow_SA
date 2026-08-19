using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Domain.Models;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.FundingRequestReview.ApproveFundingRequest
{
    // This is the handler CreateFundingRequestCommandHandler's header comment
    // flagged as "a separate, not-yet-built slice" -- SRS 3.3: a Credit Analyst
    // approves a Pending FundingRequest, which is what actually turns it into a
    // FundingCampaign + MarketplaceListing that investors can see and fund.
    public class ApproveFundingRequestCommandHandler : IRequestHandler<ApproveFundingRequestCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public ApproveFundingRequestCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(ApproveFundingRequestCommand request, CancellationToken cancellationToken)
        {
            var fundingRequest = await _context.FundingRequests
                .FirstOrDefaultAsync(r => r.FundingRequestId == request.FundingRequestId, cancellationToken);

            if (fundingRequest is null)
                throw new NotFoundException("Funding request not found.");

            if (fundingRequest.Status != FundingRequestStatus.Pending
                && fundingRequest.Status != FundingRequestStatus.UnderReview)
                throw new ConflictException("Only Pending or UnderReview funding requests can be approved.");

            // SingleInvestor/Fractional campaigns need a promised rate at listing time;
            // Auction campaigns derive it later from AuctionBid.ProposedReturnRate.
            if (fundingRequest.FundingModel != FundingModel.Auction && request.ExpectedReturnRate is null)
                throw new ConflictException(
                    "An expected return rate is required to approve a SingleInvestor or Fractional funding request.");

            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceId == fundingRequest.InvoiceId, cancellationToken);

            if (invoice is null)
                throw new NotFoundException("Invoice not found.");

            // SRS 5.11: every listed invoice must have a RiskAssessment. This should
            // always exist by the time a request reaches here, since it's produced
            // during invoice approval -- but don't silently list an unscored invoice.
            var riskAssessment = await _context.RiskAssessments
                .Where(a => a.InvoiceId == invoice.InvoiceId)
                .OrderByDescending(a => a.AssessedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (riskAssessment is null)
                throw new ConflictException(
                    "This invoice has no risk assessment yet and cannot be listed on the marketplace.");

            var sme = await _context.SMEs
                .FirstOrDefaultAsync(s => s.SMEId == fundingRequest.SMEId, cancellationToken);

            if (sme is null)
                throw new NotFoundException("SME not found.");

            var tenorDays = Math.Max(1, (invoice.DueDate.Date - DateTime.UtcNow.Date).Days);
            var fundingDeadline = request.FundingDeadline ?? DateTime.UtcNow.AddDays(14);

            var campaign = new FundingCampaign
            {
                CampaignId = Guid.NewGuid(),
                FundingRequestId = fundingRequest.FundingRequestId,
                InvoiceId = invoice.InvoiceId,
                SMEId = fundingRequest.SMEId,
                FundingModel = fundingRequest.FundingModel,
                TargetAmount = fundingRequest.RequestedAmount,
                ExpectedReturnRate = fundingRequest.FundingModel == FundingModel.Auction
                    ? null
                    : request.ExpectedReturnRate,
                FundedAmount = 0,
                TenorDays = tenorDays,
                Status = CampaignStatus.Listed,
                ListedAt = DateTime.UtcNow,
                FundingDeadline = fundingDeadline
            };

            _context.FundingCampaigns.Add(campaign);

            var listing = new MarketplaceListing
            {
                ListingId = Guid.NewGuid(),
                CampaignId = campaign.CampaignId,
                RiskGrade = riskAssessment.RiskGrade,
                RiskScore = riskAssessment.RiskScore,
                Industry = sme.Industry,
                PublishedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.MarketplaceListings.Add(listing);

            fundingRequest.Status = FundingRequestStatus.Approved;
            fundingRequest.DecisionAt = DateTime.UtcNow;
            fundingRequest.ReviewerId = request.ReviewerId;

            await _context.SaveChangesAsync(cancellationToken);

            return campaign.CampaignId;
        }
    }
}
