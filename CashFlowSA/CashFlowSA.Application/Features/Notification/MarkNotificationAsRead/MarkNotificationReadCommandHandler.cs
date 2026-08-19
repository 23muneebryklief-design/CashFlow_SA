using CashFlowSA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Notification.MarkNotificationRead
{
    public sealed class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public MarkNotificationReadCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
        {
            var notification = await _context.Notifications
                .SingleOrDefaultAsync(
                    n => n.NotificationId == request.NotificationId && n.UserId == request.UserId,
                    cancellationToken);

            if (notification is null)
                return false;

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }

            return true;
        }
    }
}
