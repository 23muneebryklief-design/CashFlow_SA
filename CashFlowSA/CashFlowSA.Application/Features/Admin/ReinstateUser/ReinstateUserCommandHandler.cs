using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Admin.ReinstateUser;

public sealed class ReinstateUserCommandHandler : IRequestHandler<ReinstateUserCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ReinstateUserCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(ReinstateUserCommand request, CancellationToken cancellationToken)
    {
        var actorId = _currentUserService.UserId;
        if (!actorId.HasValue)
            throw new AuthenticationFailedException("Authenticated administrator context is required.");

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (user is null)
            throw new NotFoundException("User not found.");

        if (user.Status != AccountStatus.Suspended)
            throw new ConflictException("Only suspended user accounts can be reinstated.");

        user.Status = AccountStatus.Active;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedByUserId = actorId.Value;

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
