using CashFlowSA.Domain.Models.Enums;
using MediatR;

namespace CashFlowSA.Application.Features.Funding.CreateFundingRequest
{
    public class CreateFundingRequestCommand : IRequest<Guid>
    {
        public Guid InvoiceId { get; set; }
        public decimal RequestedAmount { get; set; }
        public FundingModel FundingModel { get; set; }

        // Set by the authenticated API layer. Clients must not be trusted to choose this.
        public Guid SMEId { get; set; }
    }
}
