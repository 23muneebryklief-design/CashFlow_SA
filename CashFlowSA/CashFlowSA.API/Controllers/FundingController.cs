using MediatR;
using Microsoft.AspNetCore.Mvc;
using CashFlowSA.Application.Features.Funding.CommitSingleInvestorFunding;
using CashFlowSA.Application.Features.Funding.CommitFractionalFunding;
using CashFlowSA.Application.Features.Funding.PlaceAuctionBid;
using CashFlowSA.Application.Features.Funding.GetCampaignStatus;

namespace CashFlowSA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FundingController : ControllerBase
    {
        private readonly IMediator _mediator;
        public FundingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("single-investor/{listingId}/commit")]
        public async Task<IActionResult> CommitSingleInvestor(
            Guid listingId,
            [FromBody] CommitSingleInvestorFundingCommand command,
            CancellationToken cancellationToken)
        {
            command.CampaignId = listingId;
            var investmentId = await _mediator.Send(command, cancellationToken);
            return Ok(new { InvestmentId = investmentId });
        }

        [HttpPost("fractional/{listingId}/commit")]
        public async Task<IActionResult> CommitFractional(
            Guid listingId,
            [FromBody] CommitFractionalFundingCommand command,
            CancellationToken cancellationToken)
        {
            command.CampaignId = listingId;
            var investmentId = await _mediator.Send(command, cancellationToken);
            return Ok(new { InvestmentId = investmentId });
        }

        [HttpPost("auction/{listingId}/bid")]
        public async Task<IActionResult> PlaceAuctionBid(
            Guid listingId,
            [FromBody] PlaceAuctionBidCommand command,
            CancellationToken cancellationToken)
        {
            command.CampaignId = listingId;
            var bidId = await _mediator.Send(command, cancellationToken);
            return Ok(new { BidId = bidId });
        }

        [HttpGet("campaign/{campaignId}/status")]
        public async Task<IActionResult> GetCampaignStatus(
            Guid campaignId,
            CancellationToken cancellationToken)
        {
            var query = new GetCampaignStatusQuery { CampaignId = campaignId };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
    }
}
