using CashFlowSA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Notification.GetNotificationHistory
{
    public class GetNotificationHistoryQueryHandler : IRequestHandler<GetNotificationHistoryQuery, List<NotificationDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetNotificationHistoryQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<NotificationDto>> Handle(GetNotificationHistoryQuery request, CancellationToken cancellationToken)
        {
            // No NotFoundException: zero notifications is a normal state, not an error.
            return await _context.Notifications
                .Where(n => n.UserId == request.UserId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    Event = n.Event,
                    Channel = n.Channel,
                    Title = n.Title,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    ReadAt = n.ReadAt,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync(cancellationToken);
        }
    }
}
