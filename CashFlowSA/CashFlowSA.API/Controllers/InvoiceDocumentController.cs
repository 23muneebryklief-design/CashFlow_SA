using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CashFlowSA.Application.Features.Invoice.GetInvoiceDocumentDownloadUrl;

namespace CashFlowSA.API.Controllers
{
    [ApiController]
    [Route("api/invoice")]
    [Authorize(Roles = "SME,CreditAnalyst,Admin,SuperAdmin")]
    public class InvoiceDocumentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InvoiceDocumentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{invoiceId}/document/download-url")]
        public async Task<IActionResult> GetDownloadUrl(
            Guid invoiceId,
            CancellationToken cancellationToken)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value
                       ?? User.FindFirst("role")?.Value
                       ?? string.Empty;

            Guid? smeId = null;
            if (string.Equals(role, "SME", StringComparison.OrdinalIgnoreCase))
            {
                var rawProfileId = User.FindFirst("profileId")?.Value;
                if (!Guid.TryParse(rawProfileId, out var parsedSmeId))
                    return Unauthorized();
                smeId = parsedSmeId;
            }

            var query = new GetInvoiceDocumentDownloadUrlQuery
            {
                InvoiceId = invoiceId,
                SmeId = smeId,
                Role = role
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
    }
}
