using MediatR;

namespace CashFlowSA.Application.Features.Notification.GetNotificationHistory
{
    public class GetNotificationHistoryQuery : IRequest<List<NotificationDto>>
    {
        public Guid UserId { get; set; }
    }
}
