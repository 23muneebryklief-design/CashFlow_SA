using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CashFlowSA.Application.Features.InvoiceReview.GetInvoicesForReview;
using CashFlowSA.Application.Features.InvoiceReview.ApproveInvoice;
using CashFlowSA.Application.Features.InvoiceReview.RejectInvoice;
using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.API.Controllers
{
    // Fills the SRS 5.3 gap: nothing previously moved an invoice past
    // Submitted. A Credit Analyst/Admin reviews here; Approved is the
    // prerequisite CreateFundingRequestCommandHandler already checks for.
    [ApiController]
    [Route("api/invoice-review")]
    [Authorize(Roles = "CreditAnalyst,Admin")]
    public class InvoiceReviewController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InvoiceReviewController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetForReview(
            [FromQuery] InvoiceStatus? status,
            CancellationToken cancellationToken)
        {
            var query = new GetInvoicesForReviewQuery { StatusFilter = status };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpPost("{invoiceId}/approve")]
        public async Task<IActionResult> Approve(
            Guid invoiceId,
            [FromBody] ApproveInvoiceCommand command,
            CancellationToken cancellationToken)
        {
            command.InvoiceId = invoiceId;
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpPost("{invoiceId}/reject")]
        public async Task<IActionResult> Reject(
            Guid invoiceId,
            [FromBody] RejectInvoiceCommand command,
            CancellationToken cancellationToken)
        {
            command.InvoiceId = invoiceId;
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
    }
}
