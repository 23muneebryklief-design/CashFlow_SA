using MediatR;

namespace CashFlowSA.Application.Features.Investment.GetInvestorInvestments;

public sealed class GetInvestorInvestmentsQuery : IRequest<IReadOnlyList<InvestorInvestmentsDto>>
{
    public Guid InvestorId { get; set; }
}
