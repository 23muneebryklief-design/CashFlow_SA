using CashFlowSA.Application.Features.InvoiceReview.Dtos;
using CashFlowSA.Domain.Models.Enums;
using MediatR;

namespace CashFlowSA.Application.Features.InvoiceReview.GetInvoicesForReview
{
    public class GetInvoicesForReviewQuery : IRequest<List<InvoiceForReviewDto>>
    {
        // Defaults to Submitted (the review queue) when omitted -- pass a
        // specific status to see Approved/Rejected history instead.
        public InvoiceStatus? StatusFilter { get; set; }
    }
}
