using MediatR;

namespace CashFlowSA.Application.Features.Invoice.GetInvoiceDocumentDownloadUrl
{
    public class GetInvoiceDocumentDownloadUrlQuery : IRequest<InvoiceDocumentDownloadUrlDto>
    {
        public Guid InvoiceId { get; set; }
        public Guid? SmeId { get; set; }
        public string Role { get; set; } = string.Empty;
    }

    public class InvoiceDocumentDownloadUrlDto
    {
        public string Url { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string FileName { get; set; } = string.Empty;
    }
}
