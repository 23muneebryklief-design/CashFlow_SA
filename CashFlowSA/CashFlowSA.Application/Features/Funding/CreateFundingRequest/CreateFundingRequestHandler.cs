using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Domain.Models;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Funding.CreateFundingRequest
{
    // SRS 3.3: an SME requests financing against an already-Approved invoice.
    // A Credit Analyst must review and approve this request (UnderwritingReview)
    // before a FundingCampaign + MarketplaceListing get created -- that's a
    // separate, not-yet-built slice. This command only creates the Pending request.
    public class CreateFundingRequestCommandHandler : IRequestHandler<CreateFundingRequestCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public CreateFundingRequestCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CreateFundingRequestCommand request, CancellationToken cancellationToken)
        {
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceId == request.InvoiceId, cancellationToken);

            if (invoice is null)
                throw new NotFoundException("Invoice not found.");

            if (invoice.Status != InvoiceStatus.Approved)
                throw new ConflictException("Only Approved invoices can have financing requested against them.");

            // Real-world factoring rarely advances 100% of an invoice's value --
            // this allows requesting up to (but not more than) the invoice's Amount.
            if (request.RequestedAmount > invoice.Amount)
                throw new ConflictException("Requested amount cannot exceed the invoice's amount.");

            var hasPendingRequest = await _context.FundingRequests
                .AnyAsync(r => r.InvoiceId == request.InvoiceId && r.Status == FundingRequestStatus.Pending,
                    cancellationToken);

            if (hasPendingRequest)
                throw new ConflictException("This invoice already has a pending funding request.");

            var fundingRequest = new FundingRequest
            {
                FundingRequestId = Guid.NewGuid(),
                InvoiceId = invoice.InvoiceId,
                SMEId = invoice.SMEId,
                RequestedAmount = request.RequestedAmount,
                FundingModel = request.FundingModel,
                Status = FundingRequestStatus.Pending,
                SubmittedAt = DateTime.UtcNow
            };

            _context.FundingRequests.Add(fundingRequest);

            await _context.SaveChangesAsync(cancellationToken);

            return fundingRequest.FundingRequestId;
        }
    }
}