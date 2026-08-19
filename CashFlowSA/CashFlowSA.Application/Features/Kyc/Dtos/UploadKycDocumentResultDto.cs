namespace CashFlowSA.Application.Features.Kyc.DTO
{
    public class UploadKycDocumentResultDto
    {
        public Guid DocumentId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
    }
}