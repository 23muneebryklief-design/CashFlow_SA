using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CashFlowSA.Application.Features.FundingRequestReview.GetFundingRequestsForReview;
using CashFlowSA.Application.Features.FundingRequestReview.ApproveFundingRequest;
using CashFlowSA.Application.Features.FundingRequestReview.RejectFundingRequest;
using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.API.Controllers
{
    // Underwriting step described in CreateFundingRequestCommandHandler's header
    // comment (SRS 3.3): a Credit Analyst reviews a Pending FundingRequest here.
    // Approving is what actually creates the FundingCampaign + MarketplaceListing
    // that show up on InvestorMarketplace -- until this runs, a funding request
    // just sits there with nothing downstream.
    [ApiController]
    [Route("api/funding-request-review")]
    [Authorize(Roles = "CreditAnalyst,Admin")]
    public class FundingRequestReviewController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FundingRequestReviewController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetForReview(
            [FromQuery] FundingRequestStatus? status,
            CancellationToken cancellationToken)
        {
            var query = new GetFundingRequestsForReviewQuery { StatusFilter = status };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpPost("{fundingRequestId}/approve")]
        public async Task<IActionResult> Approve(
            Guid fundingRequestId,
            [FromBody] ApproveFundingRequestCommand command,
            CancellationToken cancellationToken)
        {
            command.FundingRequestId = fundingRequestId;
            var campaignId = await _mediator.Send(command, cancellationToken);
            return Ok(new { CampaignId = campaignId });
        }

        [HttpPost("{fundingRequestId}/reject")]
        public async Task<IActionResult> Reject(
            Guid fundingRequestId,
            [FromBody] RejectFundingRequestCommand command,
            CancellationToken cancellationToken)
        {
            command.FundingRequestId = fundingRequestId;
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
    }
}
