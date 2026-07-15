using FluentValidation;

namespace CashFlowSA.Application.Features.Auth.Commands.RegisterSme
{
    public class RegisterSmeCommandValidator : AbstractValidator<RegisterSmeCommand>
    {
        public RegisterSmeCommandValidator()
        {
            // User account fields
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(100);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.")
                .MaximumLength(256);

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .MaximumLength(30);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one number.");

            // SME profile fields
            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("Company name is required.")
                .MaximumLength(200);

            RuleFor(x => x.ContactPerson)
                .NotEmpty().WithMessage("Contact person is required.")
                .MaximumLength(200);

            RuleFor(x => x.CompanyEmail)
                .NotEmpty().WithMessage("Company email is required.")
                .EmailAddress().WithMessage("A valid company email address is required.")
                .MaximumLength(256);

            RuleFor(x => x.CompanyPhoneNumber)
                .NotEmpty().WithMessage("Company phone number is required.")
                .MaximumLength(30);

            RuleFor(x => x.RegistrationNumber)
                .NotEmpty().WithMessage("Company registration number is required.")
                .MaximumLength(100);

            RuleFor(x => x.TaxNumber)
                .NotEmpty().WithMessage("Tax number is required.")
                .MaximumLength(100);

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required.")
                .MaximumLength(500);

            RuleFor(x => x.Industry)
                .IsInEnum().WithMessage("A valid industry must be selected.");
        }
    }
}