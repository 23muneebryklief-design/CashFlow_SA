using MediatR;
using CashFlowSA.Application.Features.Kyc.DTO;

namespace CashFlowSA.Application.Features.Kyc.SubmitKycApplication
{
    public class SubmitKycApplicationCommand : IRequest<Guid>
    {
        public Guid SMEId { get; set; }
        public Guid UserId { get; set; }
        public List<KycDocumentDto> Documents { get; set; } = new();
    }
}