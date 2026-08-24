using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CashFlowSA.Application.Features.InvoiceReview.ApproveInvoice;
using CashFlowSA.Application.Features.InvoiceReview.GetInvoicesForReview;
using CashFlowSA.Application.Features.InvoiceReview.RejectInvoice;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CashFlowSA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SuperAdmin,CreditAnalyst")]
    public class InvoiceReviewController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InvoiceReviewController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // ============================================================
        // GET /api/InvoiceReview?status=Submitted
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> GetForReview(
            [FromQuery] InvoiceStatus? status,
            CancellationToken cancellationToken)
        {
            var query = new GetInvoicesForReviewQuery
            {
                StatusFilter = status
            };

            var result = await _mediator.Send(
                query,
                cancellationToken);

            return Ok(result);
        }

        // ============================================================
        // POST /api/InvoiceReview/{invoiceId}/approve
        // ============================================================

        [HttpPost("{invoiceId:guid}/approve")]
        public async Task<IActionResult> Approve(
            Guid invoiceId,
            [FromBody] ApproveInvoiceCommand command,
            CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var reviewerId))
            {
                return Unauthorized(
                    "The authenticated user's ID could not be determined.");
            }

            // Always use the authenticated user.
            // Do not trust reviewerId supplied by the frontend.
            command.InvoiceId = invoiceId;
            command.ReviewerId = reviewerId;

            await _mediator.Send(
                command,
                cancellationToken);

            return NoContent();
        }

        // ============================================================
        // POST /api/InvoiceReview/{invoiceId}/reject
        // ============================================================

        [HttpPost("{invoiceId:guid}/reject")]
        public async Task<IActionResult> Reject(
            Guid invoiceId,
            [FromBody] RejectInvoiceCommand command,
            CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var reviewerId))
            {
                return Unauthorized(
                    "The authenticated user's ID could not be determined.");
            }

            // Always use the authenticated user.
            // Do not trust reviewerId supplied by the frontend.
            command.InvoiceId = invoiceId;
            command.ReviewerId = reviewerId;

            await _mediator.Send(
                command,
                cancellationToken);

            return NoContent();
        }

        // ============================================================
        // GET AUTHENTICATED USER ID
        // ============================================================

        private bool TryGetAuthenticatedUserId(out Guid userId)
        {
            var rawUserId =
                User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("userId")?.Value
                ?? User.FindFirst("sub")?.Value;

            return Guid.TryParse(rawUserId, out userId);
        }
    }
}