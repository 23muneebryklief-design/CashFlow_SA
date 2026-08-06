using FluentValidation;

namespace CashFlowSA.Application.Features.Funding.CreateFundingRequest
{
    public class CreateFundingRequestCommandValidator : AbstractValidator<CreateFundingRequestCommand>
    {
        public CreateFundingRequestCommandValidator()
        {
            RuleFor(x => x.InvoiceId)
                .NotEmpty().WithMessage("Invoice ID is required.");

            RuleFor(x => x.RequestedAmount)
                .GreaterThan(0).WithMessage("Requested amount must be greater than zero.");

            RuleFor(x => x.FundingModel)
                .IsInEnum().WithMessage("A valid funding model must be selected.");
        }
    }
}