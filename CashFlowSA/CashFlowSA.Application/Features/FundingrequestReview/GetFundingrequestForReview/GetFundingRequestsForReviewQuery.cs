using CashFlowSA.Application.Features.FundingRequestReview.Dtos;
using CashFlowSA.Domain.Models.Enums;
using MediatR;

namespace CashFlowSA.Application.Features.FundingRequestReview.GetFundingRequestsForReview
{
    public class GetFundingRequestsForReviewQuery : IRequest<List<FundingRequestForReviewDto>>
    {
        public FundingRequestStatus? StatusFilter { get; set; }
    }
}
