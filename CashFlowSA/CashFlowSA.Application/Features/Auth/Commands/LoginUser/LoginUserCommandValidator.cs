using FluentValidation;

namespace CashFlowSA.Application.Features.Auth.Commands.LoginUser
{
    public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
    {
        public LoginUserCommandValidator()
        {
        RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.")
                .MaximumLength(256);
        RuleFor(x=> x.Password)
                .NotEmpty().WithMessage("Password is required");
        }
    }
}
