using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Investment.GetInvestorInvestments;

public sealed class GetInvestorInvestmentsQueryHandler
    : IRequestHandler<GetInvestorInvestmentsQuery, IReadOnlyList<InvestorInvestmentsDto>>
{
    private readonly IApplicationDbContext _context;

    public GetInvestorInvestmentsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<InvestorInvestmentsDto>> Handle(
        GetInvestorInvestmentsQuery request,
        CancellationToken cancellationToken)
    {
        if (!await _context.Investors.AnyAsync(i => i.InvestorId == request.InvestorId, cancellationToken))
            throw new NotFoundException("Investor profile not found.");

        return await (
            from investment in _context.Investments
            join campaign in _context.FundingCampaigns on investment.CampaignId equals campaign.CampaignId
            join listing in _context.MarketplaceListings on campaign.CampaignId equals listing.CampaignId
            where investment.InvestorId == request.InvestorId
            orderby investment.InvestedAt descending
            select new InvestorInvestmentsDto
            {
                InvestmentId = investment.InvestmentId,
                CampaignId = investment.CampaignId,
                Industry = listing.Industry.ToString(),
                Amount = investment.Amount,
                Status = investment.Status.ToString(),
                InvestedAt = investment.InvestedAt,
                TenorDays = campaign.TenorDays,
                ReturnAmount = investment.ReturnAmount
            }
        ).ToListAsync(cancellationToken);
    }
}
