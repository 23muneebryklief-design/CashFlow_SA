using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using CashFlowSA.Application.Features.Funding.CommitSingleInvestorFunding;
using CashFlowSA.Application.Features.Funding.CommitFractionalFunding;
using CashFlowSA.Application.Features.Funding.PlaceAuctionBid;
using CashFlowSA.Application.Features.Funding.GetCampaignStatus;
using CashFlowSA.Application.Features.Funding.CreateFundingRequest;
using CashFlowSA.Application.Features.Funding.GetMyFundingRequests;
using Microsoft.AspNetCore.Authorization;

namespace CashFlowSA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles= "Investor")]
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

        // Overrides the controller-level [Authorize(Roles="Investor")] --
        // requesting financing is an SME action, not an Investor one.
        [HttpPost("request")]
        [Authorize(Roles = "SME")]
        public async Task<IActionResult> CreateFundingRequest(
            [FromBody] CreateFundingRequestCommand command,
            CancellationToken cancellationToken)
        {
            if (!TryGetSmeId(out var smeId))
                return Unauthorized("SME profile could not be determined from the authenticated user.");

            command.SMEId = smeId;
            var fundingRequestId = await _mediator.Send(command, cancellationToken);
            return Ok(new { FundingRequestId = fundingRequestId });
        }

        [HttpGet("my")]
        [Authorize(Roles = "SME")]
        public async Task<IActionResult> GetMyFundingRequests(CancellationToken cancellationToken)
        {
            if (!TryGetSmeId(out var smeId))
                return Unauthorized("SME profile could not be determined from the authenticated user.");

            var query = new GetMyFundingRequestsQuery { SMEId = smeId };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
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
    private bool TryGetSmeId(out Guid smeId)
    {
        var claim = User.FindFirst("profileId")?.Value;
        return Guid.TryParse(claim, out smeId);
    }
    }
}

