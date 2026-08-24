using MediatR;

namespace CashFlowSA.Application.Features.Admin.SuspendUser;

public sealed class SuspendUserCommand : IRequest<Unit>
{
    public Guid UserId { get; init; }
}
