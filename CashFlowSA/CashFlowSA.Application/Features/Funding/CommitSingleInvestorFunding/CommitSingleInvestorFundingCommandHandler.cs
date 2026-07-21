using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Domain.Models;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Funding.CommitSingleInvestorFunding
{
    // SRS 5.5 Single-Investor model: one investor funds the entire campaign in one commit.
    public class CommitSingleInvestorFundingCommandHandler : IRequestHandler<CommitSingleInvestorFundingCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public CommitSingleInvestorFundingCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CommitSingleInvestorFundingCommand request, CancellationToken cancellationToken)
        {
            var campaign = await _context.FundingCampaigns
                .FirstOrDefaultAsync(c => c.CampaignId == request.CampaignId, cancellationToken);

            if (campaign is null)
                throw new NotFoundException("Funding campaign not found.");

            if (campaign.FundingModel != FundingModel.SingleInvestor)
                throw new ConflictException("This campaign is not configured for single-investor funding.");

            if (campaign.Status != CampaignStatus.Listed)
                throw new ConflictException("This campaign is not open for commitments.");

            // ASSUMPTION: single-investor funding must cover the full remaining
            // target amount in one commit -- partial single-investor commits
            // aren't defined in the SRS, so we reject anything else.
            if (request.Amount != campaign.TargetAmount - campaign.FundedAmount)
                throw new ConflictException("Amount must exactly cover the campaign's remaining target amount.");

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
            campaign.Status = CampaignStatus.Funded;

            await _context.SaveChangesAsync(cancellationToken);

            return investment.InvestmentId;
        }
    }
}
