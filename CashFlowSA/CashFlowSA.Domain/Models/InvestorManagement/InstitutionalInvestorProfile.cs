using CashFlowSA.Domain.Models;

namespace CashFlowSA.Domain.Models
{
    public class InstitutionalInvestorProfile : BaseEntity
    {
        public Guid InstitutionalInvestorProfileId { get; set; }
        public Guid InvestorId { get; set; }

        public string InstitutionName { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string FSCALicenseNumber { get; set; } = string.Empty;
        public string AuthorizedSignatoryName { get; set; } = string.Empty;
        public string AuthorizedSignatoryIdNumber {get;set;} = string.Empty;
    }
}