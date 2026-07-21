using FluentValidation;

namespace CashFlowSA.Application.Features.Notification.GetNotificationHistory
{
    public class GetNotificationHistoryQueryValidator : AbstractValidator<GetNotificationHistoryQuery>
    {
        public GetNotificationHistoryQueryValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");
        }
    }
}
