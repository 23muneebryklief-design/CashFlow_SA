using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Application.Features.Funding.Common;
using CashFlowSA.Domain.Models;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Funding.CommitFractionalFunding
{
    // SRS 5.5 Fractional model: many investors can each commit a partial amount.
    // Concurrency control (SRS 5.5 AC: never over-fund a campaign) is enforced via
    // FundingCampaign.RowVersion, an EF Core [Timestamp] concurrency token.
    public class CommitFractionalFundingCommandHandler : IRequestHandler<CommitFractionalFundingCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public CommitFractionalFundingCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CommitFractionalFundingCommand request, CancellationToken cancellationToken)
        {
            var campaign = await _context.FundingCampaigns
                .FirstOrDefaultAsync(c => c.CampaignId == request.CampaignId, cancellationToken);

            if (campaign is null)
                throw new NotFoundException("Funding campaign not found.");

            if (campaign.FundingModel != FundingModel.Fractional)
                throw new ConflictException("This campaign is not configured for fractional funding.");

            if (campaign.Status != CampaignStatus.Listed && campaign.Status != CampaignStatus.Funding)
                throw new ConflictException("This campaign is not open for commitments.");

            if (campaign.FundedAmount + request.Amount > campaign.TargetAmount)
                throw new ConflictException("Amount exceeds the campaign's remaining target amount.");

            var investment = new Investment
            {
                InvestmentId = Guid.NewGuid(),
                CampaignId = campaign.CampaignId,
                InvestorId = request.InvestorId,
                Amount = request.Amount,
                Status = InvestmentStatus.Committed,
                InvestedAt = DateTime.UtcNow
            };

            _context.Investments.Add(investment);

            campaign.FundedAmount += request.Amount;
            campaign.Status = campaign.FundedAmount >= campaign.TargetAmount
                ? CampaignStatus.Funded
                : CampaignStatus.Funding;

            // Only credit the SME on the commit that actually completes funding --
            // a campaign can only transition to Funded once in its lifecycle (the
            // status check earlier in this handler already blocks commits against
            // an already-Funded campaign), so this fires exactly once per campaign.
            if (campaign.Status == CampaignStatus.Funded)
                await SmeFundingCredit.CreditSmeWalletAsync(_context, campaign, cancellationToken);

            try
            {
                // EF Core compares campaign.RowVersion against the DB's current value.
                // If another commit updated FundedAmount since we read it, this throws
                // DbUpdateConcurrencyException instead of silently overwriting -- the
                // over-funding race the SRS calls out.
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException(
                    "This campaign was updated by another commitment at the same time. Please retry.");
            }

            return investment.InvestmentId;
        }
    }
}