using Amazon.S3;
using Amazon.S3.Model;
using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Settings;
using Microsoft.Extensions.Options;

namespace CashFlowSA.Infrastructure.Services
{
    // Talks to Supabase Storage via its S3-compatible API using the
    // official AWSSDK.S3 client, rather than hand-rolled REST calls.
    // Supabase's S3 protocol requires path-style addressing (ForcePathStyle),
    // unlike real AWS S3 which defaults to virtual-hosted-style.
    public class SupabaseFileStorage : IFileStorage
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public SupabaseFileStorage(IOptions<SupabaseStorageSettings> settings)
        {
            var config = settings.Value;
            _bucketName = config.BucketName;

            var s3Config = new AmazonS3Config
            {
                ServiceURL = config.Endpoint,
                AuthenticationRegion = config.Region,
                ForcePathStyle = true
            };

            _s3Client = new AmazonS3Client(config.AccessKey, config.SecretKey, s3Config);
        }

        public async Task<UploadedFileResult> UploadAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            CancellationToken cancellationToken = default)
        {
            // Guid-prefixed key so two SMEs uploading "id-document.pdf" on
            // the same day never collide or overwrite each other.
            var objectKey = $"{Guid.NewGuid()}-{fileName}";

            // Captured before upload -- AutoCloseStream disposes fileStream
            // once PutObjectAsync completes, so .Length wouldn't be safe
            // to read afterward.
            var fileSize = fileStream.Length;

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = objectKey,
                InputStream = fileStream,
                ContentType = contentType,
                AutoCloseStream = true
            };

            await _s3Client.PutObjectAsync(request, cancellationToken);

            return new UploadedFileResult
            {
                FileName = fileName,
                FilePath = objectKey,
                FileSize = fileSize
            };
        }

        public Task<string> GetDownloadUrlAsync(
            string filePath,
            TimeSpan expiry,
            CancellationToken cancellationToken = default)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = filePath,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.Add(expiry)
            };

            // GetPreSignedURL is a local signature computation, not a network
            // call, so there's nothing to actually await here.
            var url = _s3Client.GetPreSignedURL(request);
            return Task.FromResult(url);
        }
    }
}