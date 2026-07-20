using MediatR;
using Microsoft.AspNetCore.Mvc;
using CashFlowSA.Application.Features.Invoice.UploadInvoice;
using CashFlowSA.Application.Features.Invoice.GetInvoice;

namespace CashFlowSA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class InvoiceController : ControllerBase
    {
        private readonly IMediator _mediator;
        public InvoiceController (IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("upload")]
        public async Task<IActionResult>UploadInvoice(
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
    }
}