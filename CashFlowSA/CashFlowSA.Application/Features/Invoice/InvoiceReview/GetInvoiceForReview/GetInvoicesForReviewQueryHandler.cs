using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Features.InvoiceReview.Dtos;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.InvoiceReview.GetInvoicesForReview
{
    public class GetInvoicesForReviewQueryHandler
        : IRequestHandler<GetInvoicesForReviewQuery, List<InvoiceForReviewDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetInvoicesForReviewQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<InvoiceForReviewDto>> Handle(
            GetInvoicesForReviewQuery request,
            CancellationToken cancellationToken)
        {
            var status = request.StatusFilter ?? InvoiceStatus.Submitted;

            var invoices = await _context.Invoices
                .Where(i => i.Status == status)
                .OrderBy(i => i.UpdatedAt ?? i.CreatedAt)
                .ToListAsync(cancellationToken);

            var smeIds = invoices.Select(i => i.SMEId).Distinct().ToList();

            var smes = await _context.SMEs
                .Where(s => smeIds.Contains(s.SMEId))
                .Select(s => new { s.SMEId, s.CompanyName })
                .ToListAsync(cancellationToken);

            return invoices
                .Select(i => new InvoiceForReviewDto
                {
                    InvoiceId = i.InvoiceId,
                    SMEId = i.SMEId,
                    CompanyName = smes.FirstOrDefault(s => s.SMEId == i.SMEId)?.CompanyName ?? "Unknown",
                    InvoiceNumber = i.InvoiceNumber,
                    DebtorName = i.DebtorName,
                    Amount = i.Amount,
                    IssueDate = i.IssueDate,
                    DueDate = i.DueDate,
                    Status = i.Status,
                    SubmittedAt = i.UpdatedAt,
                    ReviewNotes = i.ReviewNotes
                })
                .ToList();
        }
    }
}
