using FluentValidation;

namespace CashFlowSA.Application.Features.Admin.ReinstateUser;

public sealed class ReinstateUserCommandValidator : AbstractValidator<ReinstateUserCommand>
{
    public ReinstateUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
