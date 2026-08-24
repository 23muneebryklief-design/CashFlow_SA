using MediatR;

namespace CashFlowSA.Application.Features.Invoice.GetOcrResult
{
    public sealed class GetOcrResultQuery : IRequest<OcrResultDto>
    {
        public Guid InvoiceId { get; set; }
        public Guid SMEId { get; set; }
    }
}
