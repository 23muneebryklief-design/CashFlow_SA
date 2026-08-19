using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Application.Features.Funding.Common;
using CashFlowSA.Application.Features.Common;
using CashFlowSA.Domain.Models;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Data;

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
            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

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

            await InvestorWalletDebit.DebitAsync(_context, request.InvestorId, request.Amount, campaign.CampaignId, cancellationToken);

            var investment = new CashFlowSA.Domain.Models.Investment
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

            // Single-investor commits always fully fund the campaign in one step,
            // so the SME can be credited immediately.
            await SmeFundingCredit.CreditSmeWalletAsync(_context, campaign, cancellationToken);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException(
                    "This campaign or wallet was updated by another transaction at the same time. Please retry.");
            }

            return investment.InvestmentId;
        }
    }
}