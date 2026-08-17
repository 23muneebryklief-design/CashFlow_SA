using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Domain.Models;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CashFlowSA.Application.Features.InvoiceReview.ApproveInvoice
{
    public class ApproveInvoiceCommandHandler : IRequestHandler<ApproveInvoiceCommand, Unit>
    {
        private readonly IApplicationDbContext _context;
        private readonly IRiskScoringService _riskScoringService;
        private readonly IRiskExplanationService _riskExplanationService;
        private readonly ILogger<ApproveInvoiceCommandHandler> _logger;

        public ApproveInvoiceCommandHandler(
            IApplicationDbContext context,
            IRiskScoringService riskScoringService,
            IRiskExplanationService riskExplanationService,
            ILogger<ApproveInvoiceCommandHandler> logger)
        {
            _context = context;
            _riskScoringService = riskScoringService;
            _riskExplanationService = riskExplanationService;
            _logger = logger;
        }

        public async Task<Unit> Handle(ApproveInvoiceCommand request, CancellationToken cancellationToken)
        {
            // Load with SME's KYCApplications included -- OllamaRiskScoringService's
            // prompt reads sme.KYCApplications directly (rejected-KYC count), same
            // shape as the hardcoded test data in AIController.
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceId == request.InvoiceId, cancellationToken);

            if (invoice is null)
                throw new NotFoundException("Invoice not found.");

            if (invoice.Status != InvoiceStatus.Submitted)
                throw new ConflictException("Only Submitted invoices can be approved.");

            invoice.Status = InvoiceStatus.Approved;
            invoice.ReviewedByUserId = request.ReviewerId;
            invoice.ReviewedAt = DateTime.UtcNow;
            invoice.ReviewNotes = request.Notes;

            var sme = await _context.SMEs
                .Include(s => s.KYCApplications)
                .FirstOrDefaultAsync(s => s.SMEId == invoice.SMEId, cancellationToken);

            if (sme is not null)
            {
                await RunRiskAssessmentAsync(invoice, sme, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }

        // Per SRS 5.11, every listed invoice needs a RiskAssessment before it can
        // reach the marketplace. Ollama runs locally, so it can be down, slow, or
        // return malformed JSON far more easily than a hosted API -- a failure here
        // must never block invoice approval, so any exception degrades to a neutral
        // fallback instead of bubbling up and failing the whole approve action.
        private async Task RunRiskAssessmentAsync(
            CashFlowSA.Domain.Models.Invoice invoice, SME sme, CancellationToken cancellationToken)
        {
            RiskAssessment riskAssessment;

            try
            {
                var scoring = await _riskScoringService.CalculateRiskAsync(invoice, sme, cancellationToken);

                riskAssessment = new RiskAssessment
                {
                    RiskAssessmentId = Guid.NewGuid(),
                    InvoiceId = invoice.InvoiceId,
                    RiskScore = scoring.RiskScore,
                    RiskGrade = scoring.RiskGrade,
                    ScoringFactors = scoring.ScoringFactors,
                    AssessedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ollama risk scoring failed for invoice {InvoiceId}.", invoice.InvoiceId);

                riskAssessment = new RiskAssessment
                {
                    RiskAssessmentId = Guid.NewGuid(),
                    InvoiceId = invoice.InvoiceId,
                    RiskScore = 50,
                    RiskGrade = RiskGrade.C,
                    ScoringFactors = "Automated scoring unavailable -- neutral default assigned pending manual review.",
                    AssessedAt = DateTime.UtcNow
                };
            }

            _context.RiskAssessments.Add(riskAssessment);

            try
            {
                var explanation = await _riskExplanationService.GenerateExplanationAsync(
                    riskAssessment, cancellationToken);

                // GenerateExplanationAsync returns a full AIExplanation entity already --
                // make sure it's actually linked to the assessment we just created.
                explanation.AIExplanationId = explanation.AIExplanationId == Guid.Empty
                    ? Guid.NewGuid()
                    : explanation.AIExplanationId;
                explanation.RiskAssessmentId = riskAssessment.RiskAssessmentId;

                _context.AIExplanations.Add(explanation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ollama risk explanation failed for invoice {InvoiceId}.", invoice.InvoiceId);

                _context.AIExplanations.Add(new AIExplanation
                {
                    AIExplanationId = Guid.NewGuid(),
                    RiskAssessmentId = riskAssessment.RiskAssessmentId,
                    ExplanationText = "Automated risk explanation is temporarily unavailable for this invoice.",
                    InvestmentSummary = string.Empty,
                    ModelUsed = "unavailable",
                    IsAvailable = false,
                    GeneratedAt = DateTime.UtcNow
                });
            }
        }
    }
}