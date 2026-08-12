using MediatR;

namespace CashFlowSA.Application.Features.AuditorKyc.RejectKycDocument
{
    public class RejectKycDocumentCommand : IRequest<Unit>
    {
        public Guid DocumentId { get; set; }
        public Guid ReviewerId { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
