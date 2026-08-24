namespace CashFlowSA.Application.Common.Ocr
{
    public interface IInvoiceOcrQueue
    {
        ValueTask EnqueueAsync(Guid invoiceId, CancellationToken cancellationToken = default);
        IAsyncEnumerable<InvoiceOcrMessage> ReadAllAsync(CancellationToken cancellationToken = default);
    }

    public sealed class InvoiceOcrMessage
    {
        private readonly Func<ValueTask> _ack;
        private readonly Func<ValueTask> _reject;

        public InvoiceOcrMessage(Guid invoiceId, Func<ValueTask> ack, Func<ValueTask> reject)
        {
            InvoiceId = invoiceId;
            _ack = ack;
            _reject = reject;
        }

        public Guid InvoiceId { get; }
        public ValueTask AckAsync() => _ack();
        public ValueTask RejectAsync() => _reject();
    }
}
