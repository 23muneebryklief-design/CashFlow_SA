using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Features.Funding.GetMyFundingRequests.Dtos;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Funding.GetMyFundingRequests
{
    public class GetMyFundingRequestsQueryHandler
        : IRequestHandler<GetMyFundingRequestsQuery, List<MyFundingRequestDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetMyFundingRequestsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MyFundingRequestDto>> Handle(
            GetMyFundingRequestsQuery request,
            CancellationToken cancellationToken)
        {
            var requests = await _context.FundingRequests
                .AsNoTracking()
                .Where(r => r.SMEId == request.SMEId)
                .OrderByDescending(r => r.SubmittedAt)
                .ToListAsync(cancellationToken);

            if (requests.Count == 0)
                return new List<MyFundingRequestDto>();

            var requestIds = requests
                .Select(r => r.FundingRequestId)
                .ToList();

            var campaigns = await _context.FundingCampaigns
                .AsNoTracking()
                .Where(c => requestIds.Contains(c.FundingRequestId))
                .ToDictionaryAsync(c => c.FundingRequestId, cancellationToken);

            return requests.Select(r =>
            {
                campaigns.TryGetValue(r.FundingRequestId, out var campaign);

                return new MyFundingRequestDto
                {
                    FundingRequestId = r.FundingRequestId,
                    InvoiceId = r.InvoiceId,
                    RequestedAmount = r.RequestedAmount,
                    Status = r.Status,
                    CurrentStage = ResolveCurrentStage(r.Status, campaign?.Status),
                    SubmittedAt = r.SubmittedAt,
                    ReviewDate = r.DecisionAt,
                    ReviewerId = r.ReviewerId,
                    ReviewNotes = r.ReviewNotes,
                    CampaignId = campaign?.CampaignId,
                    CampaignStatus = campaign?.Status
                };
            }).ToList();
        }

        private static string ResolveCurrentStage(
            FundingRequestStatus requestStatus,
            CampaignStatus? campaignStatus)
        {
            if (campaignStatus.HasValue)
            {
                return campaignStatus.Value switch
                {
                    CampaignStatus.Funded => "Funded",
                    CampaignStatus.Settled => "Settled",
                    CampaignStatus.Expired => "Campaign Expired",
                    _ => "Campaign"
                };
            }

            return requestStatus switch
            {
                FundingRequestStatus.Pending => "Submitted",
                FundingRequestStatus.UnderReview => "Under Review",
                FundingRequestStatus.Approved => "Approved",
                FundingRequestStatus.Rejected => "Rejected",
                _ => requestStatus.ToString()
            };
        }
    }
}
