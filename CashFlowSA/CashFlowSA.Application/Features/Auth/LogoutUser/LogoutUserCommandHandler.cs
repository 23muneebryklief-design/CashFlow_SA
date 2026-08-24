using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Auth.LogoutUser;

public sealed class LogoutUserCommandHandler : IRequestHandler<LogoutUserCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public LogoutUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        var session = await _context.UserSessions
            .Where(s => s.UserId == request.UserId && s.LogoutTimestamp == null)
            .OrderByDescending(s => s.LoginTimestamp)
            .FirstOrDefaultAsync(cancellationToken);

        if (session is null)
            throw new NotFoundException("No active session was found for this user.");

        session.LogoutTimestamp = DateTime.UtcNow;
        session.RefreshToken = null;
        session.RefreshTokenExpiry = null;

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
