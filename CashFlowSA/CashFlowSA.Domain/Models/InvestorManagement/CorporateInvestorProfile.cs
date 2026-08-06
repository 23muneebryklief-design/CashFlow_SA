using CashFlowSA.Domain.Models;

namespace CashFlowSA.Domain.Models
{
    public class CorporateInvestorProfile : BaseEntity
    {
        public Guid CorporateInvestorProfileId { get; set; }
        public Guid InvestorId { get; set; }

        public string CompanyName { get; set; } = string.Empty;
        public string CompanyRegistrationNumber { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
        public string AuthorizedRepresentativeName { get; set; } = string.Empty;
        public string AuthorizedRepresentativeIdNumber {get;set;} = string.Empty;
        public string UltimateBeneficialOwnerName {get;set;} = string.Empty;
    }
}