using FluentValidation;

namespace CashFlowSA.Application.Features.Admin.SuspendUser;

public sealed class SuspendUserCommandValidator : AbstractValidator<SuspendUserCommand>
{
    public SuspendUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
