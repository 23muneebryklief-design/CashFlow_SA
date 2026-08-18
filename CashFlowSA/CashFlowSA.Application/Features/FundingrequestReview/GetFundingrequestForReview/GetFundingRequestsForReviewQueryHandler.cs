using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Features.FundingRequestReview.Dtos;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.FundingRequestReview.GetFundingRequestsForReview
{
    public class GetFundingRequestsForReviewQueryHandler
        : IRequestHandler<GetFundingRequestsForReviewQuery, List<FundingRequestForReviewDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetFundingRequestsForReviewQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<FundingRequestForReviewDto>> Handle(
            GetFundingRequestsForReviewQuery request,
            CancellationToken cancellationToken)
        {
            var status = request.StatusFilter ?? FundingRequestStatus.Pending;

            var fundingRequests = await _context.FundingRequests
                .Where(r => r.Status == status)
                .OrderBy(r => r.SubmittedAt)
                .ToListAsync(cancellationToken);

            var invoiceIds = fundingRequests.Select(r => r.InvoiceId).Distinct().ToList();

            var invoices = await _context.Invoices
                .Where(i => invoiceIds.Contains(i.InvoiceId))
                .ToListAsync(cancellationToken);

            var smeIds = invoices.Select(i => i.SMEId).Distinct().ToList();

            var smes = await _context.SMEs
                .Where(s => smeIds.Contains(s.SMEId))
                .Select(s => new { s.SMEId, s.CompanyName })
                .ToListAsync(cancellationToken);

            // One-per-invoice: an invoice can only be re-scored if resubmitted, so
            // taking the most recent assessment per invoice is safe and cheap.
            var riskAssessments = await _context.RiskAssessments
                .Where(a => invoiceIds.Contains(a.InvoiceId))
                .GroupBy(a => a.InvoiceId)
                .Select(g => g.OrderByDescending(a => a.AssessedAt).First())
                .ToListAsync(cancellationToken);

            return fundingRequests
                .Select(r =>
                {
                    var invoice = invoices.FirstOrDefault(i => i.InvoiceId == r.InvoiceId);
                    var risk = riskAssessments.FirstOrDefault(a => a.InvoiceId == r.InvoiceId);

                    return new FundingRequestForReviewDto
                    {
                        FundingRequestId = r.FundingRequestId,
                        InvoiceId = r.InvoiceId,
                        SMEId = r.SMEId,
                        CompanyName = smes.FirstOrDefault(s => s.SMEId == r.SMEId)?.CompanyName ?? "Unknown",
                        InvoiceNumber = invoice?.InvoiceNumber ?? string.Empty,
                        DebtorName = invoice?.DebtorName ?? string.Empty,
                        InvoiceAmount = invoice?.Amount ?? 0,
                        DueDate = invoice?.DueDate ?? default,
                        RequestedAmount = r.RequestedAmount,
                        FundingModel = r.FundingModel,
                        Status = r.Status,
                        SubmittedAt = r.SubmittedAt,
                        RiskScore = risk?.RiskScore,
                        RiskGrade = risk?.RiskGrade
                    };
                })
                .ToList();
        }
    }
}
