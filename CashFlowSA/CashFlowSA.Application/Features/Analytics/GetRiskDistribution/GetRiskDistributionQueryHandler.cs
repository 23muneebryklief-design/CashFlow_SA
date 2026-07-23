using CashFlowSA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Analytics.GetRiskDistribution
{
    public class GetRiskDistributionQueryHandler : IRequestHandler<GetRiskDistributionQuery, List<RiskGradeCountDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetRiskDistributionQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<RiskGradeCountDto>> Handle(GetRiskDistributionQuery request, CancellationToken cancellationToken)
        {
            return await _context.RiskAssessments
                .GroupBy(r => r.RiskGrade)
                .Select(g => new RiskGradeCountDto
                {
                    RiskGrade = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.RiskGrade)
                .ToListAsync(cancellationToken);
        }
    }
}
