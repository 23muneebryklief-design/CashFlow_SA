namespace CashFlowSA.Domain.Models.InvestorManagement
{
    public class IndividualInvestorProfile : BaseEntity
    {
        public Guid IndividualInvestorProfileId { get; set; }
        public Guid InvestorId { get; set; }

        public string IdNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string TaxNumber { get; set; } = string.Empty;
        public  SourceOfFunds SourceOfFunds { get; set; }
    }
}