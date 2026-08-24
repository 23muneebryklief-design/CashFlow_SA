using CashFlowSA.Domain.Models.Enums;
using MediatR;

namespace CashFlowSA.Application.Features.Marketplace.GetListings
{
    public class GetListingsQuery : IRequest<List<ListingSummaryDto>>
    {
        // All optional -- null means "don't filter on this field" (SRS 5.4: browse/filter/sort)
        public RiskGrade? RiskGrade { get; set; }
        public IndustryType? Industry { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public int? MinTenorDays { get; set; }
        public int? MaxTenorDays { get; set; }
    }
}
