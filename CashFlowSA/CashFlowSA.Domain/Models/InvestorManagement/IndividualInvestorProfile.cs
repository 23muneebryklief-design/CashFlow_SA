using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Domain.Models
{
    public class IndividualInvestorProfile : BaseEntity
    {
        public Guid IndividualInvestorProfileId { get; set; }
        public Guid InvestorId { get; set; }

        public string IdNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string TaxNumber { get; set; } = string.Empty;
        public SalaryRange SalaryRange { get; set; }
    }
}