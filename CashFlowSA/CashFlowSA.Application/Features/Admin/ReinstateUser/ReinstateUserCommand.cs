using MediatR;

namespace CashFlowSA.Application.Features.Admin.ReinstateUser;

public sealed class ReinstateUserCommand : IRequest<Unit>
{
    public Guid UserId { get; init; }
}
