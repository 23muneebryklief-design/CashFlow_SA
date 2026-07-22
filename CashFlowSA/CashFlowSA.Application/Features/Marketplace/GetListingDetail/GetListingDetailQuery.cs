using MediatR;

namespace CashFlowSA.Application.Features.Marketplace.GetListingDetail
{
    public class GetListingDetailQuery : IRequest<ListingDetailDto>
    {
        public Guid ListingId { get; set; }
    }
}
