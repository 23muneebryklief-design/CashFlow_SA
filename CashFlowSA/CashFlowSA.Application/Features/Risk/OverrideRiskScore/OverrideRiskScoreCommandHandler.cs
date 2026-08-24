using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Risk.OverrideRiskScore;

public sealed class OverrideRiskScoreCommandHandler : IRequestHandler<OverrideRiskScoreCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public OverrideRiskScoreCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(
        OverrideRiskScoreCommand request,
        CancellationToken cancellationToken)
    {
        var analystId = _currentUserService.UserId;
        if (!analystId.HasValue)
            throw new AuthenticationFailedException("Authenticated Credit Analyst context is required.");

        var assessment = await _context.RiskAssessments
            .FirstOrDefaultAsync(x => x.InvoiceId == request.InvoiceId, cancellationToken);

        if (assessment is null)
            throw new NotFoundException("No risk assessment exists for the specified invoice.");

        if (assessment.IsOverridden)
            throw new ConflictException("The risk assessment has already been overridden.");

        var history = new RiskScoreHistory
        {
            RiskScoreHistoryId = Guid.NewGuid(),
            InvoiceId = assessment.InvoiceId,
            PreviousScore = assessment.RiskScore,
            PreviousGrade = assessment.RiskGrade,
            NewScore = request.RiskScore,
            NewGrade = request.RiskGrade,
            ChangedByUserId = analystId.Value,
            Reason = request.Justification.Trim(),
            ChangedAt = DateTime.UtcNow
        };

        assessment.RiskScore = request.RiskScore;
        assessment.RiskGrade = request.RiskGrade;
        assessment.ScoringFactors = $"Manual Credit Analyst override. Justification: {request.Justification.Trim()}";
        assessment.IsOverridden = true;
        assessment.AssessedAt = DateTime.UtcNow;
        assessment.UpdatedAt = DateTime.UtcNow;
        assessment.UpdatedByUserId = analystId.Value;

        _context.RiskScoreHistories.Add(history);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
