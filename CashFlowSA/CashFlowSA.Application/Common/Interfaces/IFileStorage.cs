namespace CashFlowSA.Application.Common.Interfaces
{
    public interface IFileStorage
    {
        Task<UploadedFileResult> UploadAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            CancellationToken cancellationToken = default);

        // Returns a temporary, signed URL a reviewer's browser can open directly --
        // KYC documents live in a private bucket, so there's no public URL to hand out.
        Task<string> GetDownloadUrlAsync(
            string filePath,
            TimeSpan expiry,
            CancellationToken cancellationToken = default);
    }

    public class UploadedFileResult
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
    }
}