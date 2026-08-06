using MediatR;
using CashFlowSA.Application.Features.Kyc.DTO;

namespace CashFlowSA.Application.Features.Kyc
{
    public class GetKycStatusQuery : IRequest<KycStatusDto>
    {
        public Guid SMEId { get; set; }
    }
}