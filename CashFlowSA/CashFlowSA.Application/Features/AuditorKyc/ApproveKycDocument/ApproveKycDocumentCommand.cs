using MediatR;

namespace CashFlowSA.Application.Features.AuditorKyc.ApproveKycDocument
{
    public class ApproveKycDocumentCommand : IRequest<Unit>
    {
        public Guid DocumentId { get; set; }
        public Guid ReviewerId { get; set; }
        public string? Notes { get; set; }
    }
}
