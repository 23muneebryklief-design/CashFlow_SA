using MediatR;
using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Application.Features.Auth.RegisterInvestor
{
    public class RegisterInvestorCommand : IRequest <Guid>
    {
        //user Account fields 
        public string FirstName { get; set; }=string.Empty;
        public string LastName { get; set; }=string.Empty;
        public string Email { get; set; }=string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;  

        //Investor account fields 
        public string Address { get; set; } = string.Empty;

        public RiskAppetite RiskAppetite { get; set; } = RiskAppetite.Low;

    }

}