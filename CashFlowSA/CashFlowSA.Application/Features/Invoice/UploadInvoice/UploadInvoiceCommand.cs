using MediatR;

namespace CashFlowSA.Application.Features.Invoice.UploadInvoice
{
    public class UploadInvoiceCommand : IRequest<Guid>
    {
        public Guid SMEId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
    }
}