using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Admin.SuspendUser;

public sealed class SuspendUserCommandHandler : IRequestHandler<SuspendUserCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public SuspendUserCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(SuspendUserCommand request, CancellationToken cancellationToken)
    {
        var actorId = _currentUserService.UserId;
        if (!actorId.HasValue)
            throw new AuthenticationFailedException("Authenticated administrator context is required.");

        if (actorId.Value == request.UserId)
            throw new ConflictException("An administrator cannot suspend their own account.");

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (user is null)
            throw new NotFoundException("User not found.");

        if (user.Status == AccountStatus.Suspended)
            throw new ConflictException("User account is already suspended.");

        if (user.Status == AccountStatus.Deactivated)
            throw new ConflictException("A deactivated account cannot be suspended.");

        if (user.Role == UsersRoles.SuperAdmin)
            throw new ConflictException("A SuperAdmin account cannot be suspended through this workflow.");

        user.Status = AccountStatus.Suspended;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedByUserId = actorId.Value;

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
