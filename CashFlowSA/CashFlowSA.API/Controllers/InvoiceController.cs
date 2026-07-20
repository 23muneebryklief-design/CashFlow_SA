using MediatR;
using Microsoft.AspNetCore.Mvc;
using CashFlowSA.Application.Features.Invoice.UploadInvoice;
using CashFlowSA.Application.Features.Invoice;

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
    }
}