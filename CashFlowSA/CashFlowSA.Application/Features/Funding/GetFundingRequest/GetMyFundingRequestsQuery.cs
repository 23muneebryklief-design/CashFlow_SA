using CashFlowSA.Application.Features.Funding.GetMyFundingRequests.Dtos;
using MediatR;

namespace CashFlowSA.Application.Features.Funding.GetMyFundingRequests
{
    public class GetMyFundingRequestsQuery : IRequest<List<MyFundingRequestDto>>
    {
        public Guid SMEId { get; set; }
    }
}
