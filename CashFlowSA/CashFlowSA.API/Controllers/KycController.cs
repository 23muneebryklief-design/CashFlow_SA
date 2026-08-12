using MediatR;
using Microsoft.AspNetCore.Mvc;
using CashFlowSA.Application.Features.Kyc.SubmitKycApplication;
using CashFlowSA.Application.Features.Kyc;
using CashFlowSA.Application.Features.Kyc.UploadKycDocument;
using Microsoft.AspNetCore.Authorization;

namespace CashFlowSA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SME")]
    public class KycController : ControllerBase
    {
        private readonly IMediator _mediator;

        // Deliberately conservative for KYC documents -- these are official
        // paperwork, not arbitrary uploads.
        private static readonly string[] AllowedContentTypes =
        {
            "application/pdf", "image/jpeg", "image/png"
        };
        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

        public KycController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("upload-document")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(MaxFileSizeBytes)]
        public async Task<IActionResult> UploadDocument(IFormFile file, CancellationToken cancellationToken)
        {
            if (file is null || file.Length == 0)
                return BadRequest("No file was uploaded.");

            if (file.Length > MaxFileSizeBytes)
                return BadRequest("File exceeds the 10MB size limit.");

            if (!AllowedContentTypes.Contains(file.ContentType))
                return BadRequest("Only PDF, JPEG, and PNG files are accepted.");

            await using var stream = file.OpenReadStream();

            var command = new UploadKycDocumentCommand
            {
                FileStream = stream,
                FileName = file.FileName,
                ContentType = file.ContentType
            };

            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitKycApplication(
            [FromBody] SubmitKycApplicationCommand command,
            CancellationToken cancellationToken)
        {
            var applicationId = await _mediator.Send(command, cancellationToken);
            return Ok(new { ApplicationId = applicationId });
        }

        [HttpGet("status/{smeId}")]
        public async Task<IActionResult> GetKycStatus(
            Guid smeId,
            CancellationToken cancellationToken)
        {
            var query = new GetKycStatusQuery { SMEId = smeId };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
    }
}