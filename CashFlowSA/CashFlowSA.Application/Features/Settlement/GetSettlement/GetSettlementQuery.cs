using MediatR;

namespace CashFlowSA.Application.Features.Settlement.GetSettlement
{
    public class GetSettlementQuery : IRequest<SettlementDto>
    {
        public Guid SettlementId { get; set; }
    }
}
