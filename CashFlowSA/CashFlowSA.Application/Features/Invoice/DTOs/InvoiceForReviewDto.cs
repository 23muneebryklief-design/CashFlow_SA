using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Application.Features.InvoiceReview.Dtos
{
    public class InvoiceForReviewDto
    {
        public Guid InvoiceId { get; set; }
        public Guid SMEId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public string DebtorName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public InvoiceStatus Status { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string? ReviewNotes { get; set; }
    }
}
