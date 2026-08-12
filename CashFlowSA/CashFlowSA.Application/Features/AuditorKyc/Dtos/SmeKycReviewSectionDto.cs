using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Application.Features.AuditorKyc.Dtos
{
    // One "section" in the auditor's review screen: an SME plus every KYC
    // document they've uploaded, so the auditor can review a business's full
    // paperwork together instead of a flat, unattributed document list.
    public class SmeKycReviewSectionDto
    {
        public Guid SMEId { get; set; }
        public Guid UserId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public KycStatus? ApplicationStatus { get; set; }
        public List<KycDocumentReviewDto> Documents { get; set; } = new();
    }
}
