using CashFlowSA.Domain.Models.Enums;
using MediatR;

namespace CashFlowSA.Application.Features.Funding.CreateFundingRequest
{
    public class CreateFundingRequestCommand : IRequest<Guid>
    {
        public Guid InvoiceId { get; set; }
        public decimal RequestedAmount { get; set; }
        public FundingModel FundingModel { get; set; }
    }
}