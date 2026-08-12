using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Features.Kyc.DTO;
using MediatR;

namespace CashFlowSA.Application.Features.Kyc.UploadKycDocument
{
    public class UploadKycDocumentCommandHandler : IRequestHandler<UploadKycDocumentCommand, UploadKycDocumentResultDto>
    {
        private readonly IFileStorage _fileStorage;

        public UploadKycDocumentCommandHandler(IFileStorage fileStorage)
        {
            _fileStorage = fileStorage;
        }

        public async Task<UploadKycDocumentResultDto> Handle(UploadKycDocumentCommand request, CancellationToken cancellationToken)
        {
            var result = await _fileStorage.UploadAsync(
                request.FileStream,
                request.FileName,
                request.ContentType,
                cancellationToken);

            return new UploadKycDocumentResultDto
            {
                FileName = result.FileName,
                FilePath = result.FilePath,
                FileSize = result.FileSize
            };
        }
    }
}