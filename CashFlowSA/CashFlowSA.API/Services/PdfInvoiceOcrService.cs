using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;
using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Ocr;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace CashFlowSA.API.Services
{
    /// <summary>
    /// Extracts text from text-based invoice PDFs. Scanned/image-only PDFs are
    /// deliberately flagged for manual review instead of being silently accepted.
    /// </summary>
    public sealed class PdfInvoiceOcrService : IInvoiceOcrService
    {
        private readonly IFileStorage _fileStorage;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PdfInvoiceOcrService> _logger;

        public PdfInvoiceOcrService(
            IFileStorage fileStorage,
            IHttpClientFactory httpClientFactory,
            ILogger<PdfInvoiceOcrService> logger)
        {
            _fileStorage = fileStorage;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<InvoiceOcrExtraction> ExtractAsync(
            Guid invoiceId,
            string filePath,
            CancellationToken cancellationToken = default)
        {
            var url = await _fileStorage.GetDownloadUrlAsync(
                filePath,
                TimeSpan.FromMinutes(10),
                cancellationToken);

            var client = _httpClientFactory.CreateClient();
            await using var remoteStream = await client.GetStreamAsync(url, cancellationToken);
            await using var pdfStream = new MemoryStream();
            await remoteStream.CopyToAsync(pdfStream, cancellationToken);
            pdfStream.Position = 0;

            string text;
            try
            {
                using var document = PdfDocument.Open(pdfStream);
                text = string.Join("\n", document.GetPages()
             .Select(page => ContentOrderTextExtractor.GetText(page)));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to read invoice PDF {InvoiceId}.", invoiceId);
                return ManualReview();
            }

            if (string.IsNullOrWhiteSpace(text) || text.Trim().Length < 20)
                return ManualReview();

            var invoiceNumber = Match(text, new[]
            {
                @"(?im)\b(?:invoice\s*(?:no|number|#)?|inv\s*(?:no|#))\s*[:#-]?\s*([A-Z0-9][A-Z0-9./_-]{2,})"
            });

            var debtorName = Match(text, new[]
            {
                @"(?im)\b(?:bill\s*to|customer|client|debtor)\s*[:#-]?\s*([^\r\n]+)"
            });

            var amount = MatchDecimal(text, new[]
            {
                @"(?im)\b(?:total|amount\s*due|invoice\s*total|balance\s*due)\s*[:#-]?\s*(?:R|ZAR|\$)?\s*([0-9][0-9,]*(?:\.[0-9]{1,2})?)"
            });

            var dueDate = MatchDate(text, new[]
            {
                @"(?im)\b(?:due\s*date|payment\s*due)\s*[:#-]?\s*([0-9]{1,2}[/-][0-9]{1,2}[/-][0-9]{2,4})",
                @"(?im)\b(?:due\s*date|payment\s*due)\s*[:#-]?\s*([0-9]{1,2}\s+[A-Za-z]{3,9}\s+[0-9]{4})"
            });

            var found = (invoiceNumber is not null ? 1 : 0) +
                        (debtorName is not null ? 1 : 0) +
                        (amount.HasValue ? 1 : 0) +
                        (dueDate.HasValue ? 1 : 0);

            var confidence = found switch
            {
                4 => 90m,
                3 => 75m,
                2 => 55m,
                _ => 25m
            };

            return new InvoiceOcrExtraction(
                Clean(invoiceNumber),
                amount,
                dueDate,
                Clean(debtorName),
                confidence,
                found < 3);
        }

        private static InvoiceOcrExtraction ManualReview() =>
            new(null, null, null, null, 0m, true);

        private static string? Match(string text, IEnumerable<string> patterns)
        {
            foreach (var pattern in patterns)
            {
                var match = Regex.Match(text, pattern, RegexOptions.Multiline);
                if (match.Success)
                    return match.Groups[1].Value.Trim();
            }
            return null;
        }

        private static decimal? MatchDecimal(string text, IEnumerable<string> patterns)
        {
            var value = Match(text, patterns);
            if (value is null) return null;
            value = value.Replace(",", string.Empty);
            return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result)
                ? result
                : null;
        }

        private static DateTime? MatchDate(string text, IEnumerable<string> patterns)
        {
            var value = Match(text, patterns);
            if (value is null) return null;

            string[] formats =
            {
                "d/M/yyyy", "dd/MM/yyyy", "d-MM-yyyy", "dd-MM-yyyy",
                "d/M/yy", "dd/MM/yy", "d-MM-yy", "dd-MM-yy",
                "d MMM yyyy", "dd MMM yyyy", "d MMMM yyyy", "dd MMMM yyyy"
            };

            return DateTime.TryParseExact(
                value, formats, CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces, out var date)
                ? date
                : null;
        }

        private static string? Clean(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : Regex.Replace(value.Trim(), @"\s{2,}", " ");
    }
}
