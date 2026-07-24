namespace CashFlowSA.Application.Features.AdminKyc.GetPendingKycApplications
{
    public class PendingKycApplicationDto
    {
        public Guid ApplicationId { get; set; }
        public Guid SMEId { get; set; }
        public DateTime ApplicationDate { get; set; }
    }
}
