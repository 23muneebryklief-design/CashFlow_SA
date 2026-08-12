using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CashFlowSA.Application.Features.AuditorKyc.GetKycDocumentsForReview;
using CashFlowSA.Application.Features.AuditorKyc.GetKycDocumentDownloadUrl;
using CashFlowSA.Application.Features.AuditorKyc.ApproveKycDocument;
using CashFlowSA.Application.Features.AuditorKyc.RejectKycDocument;
using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.API.Controllers
{
    // Auditor-facing document review, separate from AdminKycController which
    // handles the overall application decision. Auditors work document-by-
    // document, grouped by SME, rather than approving a whole application.
    [ApiController]
    [Route("api/auditor/kyc")]
    [Authorize(Roles = "Auditor,Admin")]
    public class AuditorKycController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuditorKycController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("documents")]
        public async Task<IActionResult> GetDocumentsForReview(
            [FromQuery] DocumentStatus? status,
            CancellationToken cancellationToken)
        {
            var query = new GetKycDocumentsForReviewQuery { StatusFilter = status };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("documents/{documentId}/download-url")]
        public async Task<IActionResult> GetDownloadUrl(Guid documentId, CancellationToken cancellationToken)
        {
            var query = new GetKycDocumentDownloadUrlQuery { DocumentId = documentId };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpPost("documents/{documentId}/approve")]
        public async Task<IActionResult> ApproveDocument(
            Guid documentId,
            [FromBody] ApproveKycDocumentCommand command,
            CancellationToken cancellationToken)
        {
            command.DocumentId = documentId;
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpPost("documents/{documentId}/reject")]
        public async Task<IActionResult> RejectDocument(
            Guid documentId,
            [FromBody] RejectKycDocumentCommand command,
            CancellationToken cancellationToken)
        {
            command.DocumentId = documentId;
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
    }
}
