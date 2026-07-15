using MediatR;
using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Application.Features.Auth.Commands.RegisterSme
{
    public class RegisterSmeCommand : IRequest<Guid>
    {
        // User account fields
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // SME profile fields
        public string CompanyName { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string CompanyEmail { get; set; } = string.Empty;
        public string CompanyPhoneNumber { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public IndustryType  Industry{ get; set; }
    }
}