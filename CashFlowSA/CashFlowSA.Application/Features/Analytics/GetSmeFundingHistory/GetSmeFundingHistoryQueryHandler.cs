using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Analytics.GetSmeFundingHistory;

public sealed class GetSmeFundingHistoryQueryHandler
    : IRequestHandler<GetSmeFundingHistoryQuery, IReadOnlyList<SmeFundingHistoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSmeFundingHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SmeFundingHistoryDto>> Handle(
        GetSmeFundingHistoryQuery request,
        CancellationToken cancellationToken)
    {
        if (!request.SmeId.HasValue)
            throw new NotFoundException("SME profile could not be determined.");

        var exists = await _context.SMEs
            .AsNoTracking()
            .AnyAsync(s => s.SMEId == request.SmeId.Value, cancellationToken);

        if (!exists)
            throw new NotFoundException("SME profile not found.");

        return await _context.FundingCampaigns
            .AsNoTracking()
            .Where(c => c.SMEId == request.SmeId.Value && c.Status != CampaignStatus.Draft)
            .OrderByDescending(c => c.ListedAt ?? c.CreatedAt)
            .Select(c => new SmeFundingHistoryDto
            {
                CampaignId = c.CampaignId,
                InvoiceId = c.InvoiceId,
                TargetAmount = c.TargetAmount,
                FundedAmount = c.FundedAmount,
                Status = c.Status.ToString(),
                FundingModel = c.FundingModel.ToString(),
                TenorDays = c.TenorDays,
                ListedAt = c.ListedAt,
                FundingDeadline = c.FundingDeadline
            })
            .ToListAsync(cancellationToken);
    }
}
