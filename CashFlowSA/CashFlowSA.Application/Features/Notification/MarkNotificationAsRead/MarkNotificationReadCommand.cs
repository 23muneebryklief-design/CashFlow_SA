using MediatR;

namespace CashFlowSA.Application.Features.Notification.MarkNotificationRead
{
    public sealed class MarkNotificationReadCommand : IRequest<bool>
    {
        public Guid NotificationId { get; init; }
        public Guid UserId { get; init; }
    }
}
