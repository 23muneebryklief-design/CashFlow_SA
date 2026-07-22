using MediatR;
using Microsoft.AspNetCore.Mvc;
using CashFlowSA.Application.Features.Marketplace.GetListings;
using CashFlowSA.Application.Features.Marketplace.GetListingDetail;
using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MarketplaceController : ControllerBase
    {
        private readonly IMediator _mediator;
        public MarketplaceController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("listings")]
        public async Task<IActionResult> GetListings(
            [FromQuery] RiskGrade? riskGrade,
            [FromQuery] IndustryType? industry,
            [FromQuery] decimal? minAmount,
            [FromQuery] decimal? maxAmount,
            CancellationToken cancellationToken)
        {
            var query = new GetListingsQuery
            {
                RiskGrade = riskGrade,
                Industry = industry,
                MinAmount = minAmount,
                MaxAmount = maxAmount
            };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("listings/{listingId}")]
        public async Task<IActionResult> GetListingDetail(
            Guid listingId,
            CancellationToken cancellationToken)
        {
            var query = new GetListingDetailQuery { ListingId = listingId };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
    }
}
