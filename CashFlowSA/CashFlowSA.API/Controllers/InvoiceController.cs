using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Features.Invoice.UploadInvoice;
using CashFlowSA.Application.Features.Invoice.CorrectInvoiceFields;
using CashFlowSA.Application.Features.Invoice.SubmitInvoice;
using CashFlowSA.Application.Features.Invoice.GetInvoice;
using CashFlowSA.Application.Features.Invoice.GetInvoicesBySme;

namespace CashFlowSA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SME")]
    public class InvoiceController : ControllerBase
    {
        private static readonly string[] AllowedContentTypes =
        {
            "application/pdf"
        };

        private const long MaxFileSizeBytes = 10 * 1024 * 1024;

        private readonly IMediator _mediator;
        private readonly IFileStorage _fileStorage;

        public InvoiceController(IMediator mediator, IFileStorage fileStorage)
        {
            _mediator = mediator;
            _fileStorage = fileStorage;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(MaxFileSizeBytes)]
        public async Task<IActionResult> UploadInvoice(
            IFormFile file,
            CancellationToken cancellationToken)
        {
            if (!TryGetSmeId(out var smeId))
                return Unauthorized("SME profile could not be determined from the authenticated user.");

            if (file is null || file.Length == 0)
                return BadRequest("No invoice file was uploaded.");

            if (file.Length > MaxFileSizeBytes)
                return BadRequest("Invoice file exceeds the 10MB size limit.");

            if (!AllowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
                return BadRequest("Only PDF invoice files are accepted.");

            await using var stream = file.OpenReadStream();
            var storedFile = await _fileStorage.UploadAsync(
                stream,
                file.FileName,
                file.ContentType,
                cancellationToken);

            var command = new UploadInvoiceCommand
            {
                SMEId = smeId,
                FileName = storedFile.FileName,
                FilePath = storedFile.FilePath,
                FileSize = storedFile.FileSize
            };

            var invoiceId = await _mediator.Send(command, cancellationToken);
            return Ok(new { InvoiceId = invoiceId });
        }

        [HttpGet("{invoiceId}")]
        public async Task<IActionResult> GetInvoice(
            Guid invoiceId,
            CancellationToken cancellationToken)
        {
            if (!TryGetSmeId(out var smeId))
                return Unauthorized();

            var query = new GetInvoiceQuery
            {
                InvoiceId = invoiceId,
                SMEId = smeId
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("sme/{smeId}")]
        public async Task<IActionResult> GetInvoicesBySme(
            Guid smeId,
            CancellationToken cancellationToken)
        {
            if (!TryGetSmeId(out var authenticatedSmeId) || authenticatedSmeId != smeId)
                return Forbid();

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
            if (!TryGetSmeId(out var smeId))
                return Unauthorized();

            command.InvoiceId = invoiceId;
            command.SMEId = smeId;
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpPost("{invoiceId}/submit")]
        public async Task<IActionResult> SubmitInvoice(
            Guid invoiceId,
            CancellationToken cancellationToken)
        {
            if (!TryGetSmeId(out var smeId))
                return Unauthorized();

            var command = new SubmitInvoiceCommand
            {
                InvoiceId = invoiceId,
                SMEId = smeId
            };

            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }

        private bool TryGetSmeId(out Guid smeId)
        {
            var claim = User.FindFirst("profileId")?.Value;
            return Guid.TryParse(claim, out smeId);
        }
    }
}
