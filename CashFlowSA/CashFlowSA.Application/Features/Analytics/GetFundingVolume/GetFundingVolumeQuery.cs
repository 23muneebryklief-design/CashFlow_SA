using MediatR;

namespace CashFlowSA.Application.Features.Analytics.GetFundingVolume
{
    public class GetFundingVolumeQuery : IRequest<FundingVolumeDto>
    {
        // Optional date range -- null means "across all time".
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
