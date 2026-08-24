using MediatR;

namespace CashFlowSA.Application.Features.Auth.LogoutUser;

public sealed class LogoutUserCommand : IRequest<Unit>
{
    public Guid UserId { get; init; }
}
