using CashFlowSA.Models.enums;
namespace CashFlowSA.Models.SmeManagement
{
    public class SME
    {
        public Guid SMEId { get; set; }
        public Guid UserId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactPerson { get; set; }= string.Empty;
        public string CompanyEmail { get; set; }= string.Empty;
        public string CompanyPhoneNumber { get; set; }= string.Empty;
        public DateTime RegistrationDate { get; set; }
        public string RegistrationNumber { get; set; }=string.Empty;
        public IndustryType Industry { get; set; } = IndustryType.Other;
        public string Address { get; set; }= string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
    }
}

//Purpose:

///Represents businesses seeking financing.