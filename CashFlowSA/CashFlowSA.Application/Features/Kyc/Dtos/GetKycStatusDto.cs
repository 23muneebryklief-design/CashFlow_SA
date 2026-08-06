using CashFlowSA.Domain.Models.Enums;
namespace CashFlowSA.Application.Features.Kyc.DTO
{
    public class KycStatusDto
        {
            public Guid ApplicationId { get; set; }
            public KycStatus Status { get; set; }
            public DateTime ApplicationDate { get; set; }
        }
}