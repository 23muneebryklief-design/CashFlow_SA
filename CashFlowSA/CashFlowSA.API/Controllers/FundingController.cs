using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CashFlowSA.Application.Features.Funding.CommitSingleInvestorFunding;
using CashFlowSA.Application.Features.Funding.CommitFractionalFunding;
using CashFlowSA.Application.Features.Funding.PlaceAuctionBid;
using CashFlowSA.Application.Features.Funding.GetCampaignStatus;
using CashFlowSA.Application.Features.Funding.CreateFundingRequest;
using CashFlowSA.Application.Features.Funding.GetMyFundingRequests;

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

        // ============================================================
        // INVESTOR FUNDING
        // ============================================================

        [HttpPost("single-investor/{listingId}/commit")]
        [Authorize(Roles = "Investor")]
        public async Task<IActionResult> CommitSingleInvestor(
            Guid listingId,
            [FromBody] CommitSingleInvestorFundingCommand command,
            CancellationToken cancellationToken)
        {
            command.CampaignId = listingId;

            var investmentId = await _mediator.Send(
                command,
                cancellationToken);

            return Ok(new
            {
                InvestmentId = investmentId
            });
        }

        [HttpPost("fractional/{listingId}/commit")]
        [Authorize(Roles = "Investor")]
        public async Task<IActionResult> CommitFractional(
            Guid listingId,
            [FromBody] CommitFractionalFundingCommand command,
            CancellationToken cancellationToken)
        {
            command.CampaignId = listingId;

            var investmentId = await _mediator.Send(
                command,
                cancellationToken);

            return Ok(new
            {
                InvestmentId = investmentId
            });
        }

        [HttpPost("auction/{listingId}/bid")]
        [Authorize(Roles = "Investor")]
        public async Task<IActionResult> PlaceAuctionBid(
            Guid listingId,
            [FromBody] PlaceAuctionBidCommand command,
            CancellationToken cancellationToken)
        {
            command.CampaignId = listingId;

            var bidId = await _mediator.Send(
                command,
                cancellationToken);

            return Ok(new
            {
                BidId = bidId
            });
        }

        // ============================================================
        // SME FUNDING REQUEST
        // ============================================================

        [HttpPost("request")]
        [Authorize(Roles = "SME")]
        public async Task<IActionResult> CreateFundingRequest(
            [FromBody] CreateFundingRequestCommand command,
            CancellationToken cancellationToken)
        {
            if (!TryGetSmeId(out var smeId))
            {
                return Unauthorized(
                    "SME profile could not be determined from the authenticated user.");
            }

            // Never trust the SME ID supplied by the client.
            command.SMEId = smeId;

            var fundingRequestId = await _mediator.Send(
                command,
                cancellationToken);

            return Ok(new
            {
                FundingRequestId = fundingRequestId
            });
        }

        // ============================================================
        // SME FUNDING REQUEST STATUS
        // ============================================================

        [HttpGet("my")]
        [Authorize(Roles = "SME")]
        public async Task<IActionResult> GetMyFundingRequests(
            CancellationToken cancellationToken)
        {
            if (!TryGetSmeId(out var smeId))
            {
                return Unauthorized(
                    "SME profile could not be determined from the authenticated user.");
            }

            var query = new GetMyFundingRequestsQuery
            {
                SMEId = smeId
            };

            var result = await _mediator.Send(
                query,
                cancellationToken);

            return Ok(result);
        }

        // ============================================================
        // CAMPAIGN STATUS
        // ============================================================

        [HttpGet("campaign/{campaignId}/status")]
        [Authorize]
        public async Task<IActionResult> GetCampaignStatus(
            Guid campaignId,
            CancellationToken cancellationToken)
        {
            var query = new GetCampaignStatusQuery
            {
                CampaignId = campaignId
            };

            var result = await _mediator.Send(
                query,
                cancellationToken);

            return Ok(result);
        }

        private bool TryGetSmeId(out Guid smeId)
        {
            var claim = User.FindFirst("profileId")?.Value;

            return Guid.TryParse(
                claim,
                out smeId);
        }
    }
}
