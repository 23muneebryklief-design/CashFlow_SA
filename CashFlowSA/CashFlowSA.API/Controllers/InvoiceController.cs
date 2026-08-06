using MediatR;
using Microsoft.AspNetCore.Mvc;
using CashFlowSA.Application.Features.Invoice.UploadInvoice;
using CashFlowSA.Application.Features.Invoice.CorrectInvoiceFields;
using CashFlowSA.Application.Features.Invoice.SubmitInvoice;
using CashFlowSA.Application.Features.Invoice.GetInvoice;
using CashFlowSA.Application.Features.Invoice.GetInvoicesBySme;
using Microsoft.AspNetCore.Authorization;

namespace CashFlowSA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles ="SME")]
    public class InvoiceController : ControllerBase
    {
        private readonly IMediator _mediator;
        public InvoiceController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadInvoice(
            [FromBody] UploadInvoiceCommand command,
            CancellationToken cancellationToken)
        {
            var invoiceId = await _mediator.Send(command, cancellationToken);
            return Ok(new { InvoiceId = invoiceId });
        }

        [HttpGet("{invoiceId}")]
        public async Task<IActionResult> GetInvoice(
            Guid invoiceId,
            CancellationToken cancellationToken)
        {
            var query = new GetInvoiceQuery { InvoiceId = invoiceId };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("sme/{smeId}")]
        public async Task<IActionResult> GetInvoicesBySme(
            Guid smeId,
            CancellationToken cancellationToken)
        {
            var query = new GetInvoicesBySmeQuery { SMEId = smeId };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpPut("{invoiceId}/correct")]
        public async Task<IActionResult> CorrectInvoiceFields(
            Guid invoiceId,
            [FromBody] CorrectInvoiceFieldsCommand command,
            CancellationToken cancellationToken)
        {
            command.InvoiceId = invoiceId;
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpPost("{invoiceId}/submit")]
        public async Task<IActionResult> SubmitInvoice(
            Guid invoiceId,
            CancellationToken cancellationToken)
        {
            var command = new SubmitInvoiceCommand { InvoiceId = invoiceId };
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
    }
}
