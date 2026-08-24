using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CashFlowSA.Application.Features.Kyc;
using CashFlowSA.Application.Features.Kyc.SubmitKycApplication;
using CashFlowSA.Application.Features.Kyc.UploadKycDocument;
using CashFlowSA.Application.Features.Kyc.GetKycDocumentDownloadUrl;

namespace CashFlowSA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SME")]
    public class KycController : ControllerBase
    {
        private readonly IMediator _mediator;

        private static readonly string[] AllowedContentTypes =
        {
            "application/pdf",
            "image/jpeg",
            "image/png"
        };

        private const long MaxFileSizeBytes = 10 * 1024 * 1024;

        public KycController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("upload-document")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(MaxFileSizeBytes)]
        public async Task<IActionResult> UploadDocument(
            IFormFile file,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            if (file is null || file.Length == 0)
                return BadRequest("No file was uploaded.");

            if (file.Length > MaxFileSizeBytes)
                return BadRequest("File exceeds the 10MB size limit.");

            if (!AllowedContentTypes.Contains(
                    file.ContentType,
                    StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(
                    "Only PDF, JPEG, and PNG files are accepted.");
            }

            await using var stream = file.OpenReadStream();

            var command = new UploadKycDocumentCommand
            {
                FileStream = stream,
                FileName = file.FileName,
                ContentType = file.ContentType,
                UserId = userId
            };

            var result = await _mediator.Send(
                command,
                cancellationToken);

            return Ok(result);
        }

        [HttpGet("documents/{documentId}/download-url")]
        public async Task<IActionResult> GetDocumentDownloadUrl(
            Guid documentId,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var query = new GetKycDocumentDownloadUrlQuery
            {
                DocumentId = documentId,
                UserId = userId
            };

            var result = await _mediator.Send(
                query,
                cancellationToken);

            return Ok(result);
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitKycApplication(
            [FromBody] SubmitKycApplicationCommand command,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            command.UserId = userId;

            var applicationId = await _mediator.Send(
                command,
                cancellationToken);

            return Ok(new
            {
                ApplicationId = applicationId
            });
        }

        [HttpGet("status/{smeId}")]
        public async Task<IActionResult> GetKycStatus(
            Guid smeId,
            CancellationToken cancellationToken)
        {
            if (!TryGetSmeId(out var authenticatedSmeId) ||
                authenticatedSmeId != smeId)
            {
                return Forbid();
            }

            var query = new GetKycStatusQuery
            {
                SMEId = smeId
            };

            var result = await _mediator.Send(
                query,
                cancellationToken);

            return Ok(result);
        }

        private bool TryGetSmeId(out Guid smeId)
        {
            var claim = User.FindFirst("profileId")?.Value;

            return Guid.TryParse(claim, out smeId);
        }

        private bool TryGetUserId(out Guid userId)
        {
            var raw =
                User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(raw, out userId);
        }
    }
}