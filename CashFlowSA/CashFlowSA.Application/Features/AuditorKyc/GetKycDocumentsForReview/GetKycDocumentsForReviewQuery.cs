using CashFlowSA.Application.Features.AuditorKyc.Dtos;
using CashFlowSA.Domain.Models.Enums;
using MediatR;

namespace CashFlowSA.Application.Features.AuditorKyc.GetKycDocumentsForReview
{
    public class GetKycDocumentsForReviewQuery : IRequest<List<SmeKycReviewSectionDto>>
    {
        // Optional -- omit to see every document regardless of status, or pass
        // Pending to get just the auditor's outstanding queue.
        public DocumentStatus? StatusFilter { get; set; }
    }
}
