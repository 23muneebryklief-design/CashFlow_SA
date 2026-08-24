using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Ocr;
using CashFlowSA.Application.Common.Settings;
using CashFlowSA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CashFlowSA.API.Services
{
    public sealed class InvoiceOcrBackgroundService : BackgroundService
    {
        private readonly IInvoiceOcrQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RabbitMqSettings _settings;
        private readonly ILogger<InvoiceOcrBackgroundService> _logger;

        public InvoiceOcrBackgroundService(
            IInvoiceOcrQueue queue,
            IServiceScopeFactory scopeFactory,
            IOptions<RabbitMqSettings> options,
            ILogger<InvoiceOcrBackgroundService> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _settings = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await foreach (var delivery in _queue.ReadAllAsync(stoppingToken))
                        await ProcessDeliveryAsync(delivery, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "RabbitMQ OCR consumer stopped unexpectedly. Retrying connection.");

                    try
                    {
                        await Task.Delay(
                            TimeSpan.FromSeconds(Math.Max(1, _settings.NetworkRecoverySeconds)),
                            stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }
        }

        private async Task ProcessDeliveryAsync(
            InvoiceOcrMessage delivery,
            CancellationToken cancellationToken)
        {
            var maxAttempts = Math.Max(1, _settings.MaxProcessingAttempts);

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    await ProcessAsync(delivery.InvoiceId, cancellationToken);
                    await delivery.AckAsync();

                    _logger.LogInformation(
                        "Invoice OCR processing completed for {InvoiceId}.",
                        delivery.InvoiceId);

                    return;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // Leave the message unacknowledged so RabbitMQ can redeliver it.
                    return;
                }
                catch (Exception ex) when (attempt < maxAttempts)
                {
                    _logger.LogWarning(
                        ex,
                        "Invoice OCR attempt {Attempt}/{MaxAttempts} failed for {InvoiceId}. Retrying.",
                        attempt, maxAttempts, delivery.InvoiceId);

                    await Task.Delay(
                        TimeSpan.FromSeconds(Math.Min(5, attempt * 2)),
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Invoice OCR permanently failed for {InvoiceId} after {Attempts} attempts.",
                        delivery.InvoiceId, maxAttempts);

                    await delivery.RejectAsync();
                }
            }
        }

        private async Task ProcessAsync(Guid invoiceId, CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var ocr = scope.ServiceProvider.GetRequiredService<IInvoiceOcrService>();

            var document = await context.InvoiceDocuments
                .AsNoTracking()
                .Where(d => d.InvoiceId == invoiceId)
                .OrderByDescending(d => d.UploadedAt)
                .FirstOrDefaultAsync(cancellationToken);

            var invoice = await context.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId, cancellationToken);

            if (document is null || invoice is null)
                throw new InvalidOperationException(
                    $"OCR invoice/document {invoiceId} was not found.");

            try
            {
                var extraction = await ocr.ExtractAsync(
                    invoice.InvoiceId,
                    document.FilePath,
                    cancellationToken);

                var existing = await context.OCRResults
                    .FirstOrDefaultAsync(x => x.InvoiceId == invoiceId, cancellationToken);

                var result = existing ?? new OCRResult
                {
                    OCRResultId = Guid.NewGuid(),
                    InvoiceId = invoiceId
                };

                result.ExtractedInvoiceNumber = extraction.InvoiceNumber;
                result.ExtractedAmount = extraction.Amount;
                result.ExtractedDueDate = extraction.DueDate;
                result.ExtractedDebtorName = extraction.DebtorName;
                result.ConfidenceScore = extraction.ConfidenceScore;
                result.RequiresManualReview = extraction.RequiresManualReview;
                result.ProcessedAt = DateTime.UtcNow;

                if (existing is null)
                    context.OCRResults.Add(result);

                if (!extraction.RequiresManualReview)
                {
                    if (!string.IsNullOrWhiteSpace(extraction.InvoiceNumber))
                        invoice.InvoiceNumber = extraction.InvoiceNumber;

                    if (!string.IsNullOrWhiteSpace(extraction.DebtorName))
                        invoice.DebtorName = extraction.DebtorName;

                    if (extraction.Amount.HasValue)
                        invoice.Amount = extraction.Amount.Value;

                    if (extraction.DueDate.HasValue)
                        invoice.DueDate = extraction.DueDate.Value;
                }

                invoice.ProcessingComplete = !extraction.RequiresManualReview;
                await context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "OCR extraction failed for invoice {InvoiceId}.",
                    invoiceId);

                var existing = await context.OCRResults
                    .FirstOrDefaultAsync(x => x.InvoiceId == invoiceId, cancellationToken);

                if (existing is null)
                {
                    context.OCRResults.Add(new OCRResult
                    {
                        OCRResultId = Guid.NewGuid(),
                        InvoiceId = invoiceId,
                        ConfidenceScore = 0,
                        RequiresManualReview = true,
                        ProcessedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    existing.ConfidenceScore = 0;
                    existing.RequiresManualReview = true;
                    existing.ProcessedAt = DateTime.UtcNow;
                }

                invoice.ProcessingComplete = false;
                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
