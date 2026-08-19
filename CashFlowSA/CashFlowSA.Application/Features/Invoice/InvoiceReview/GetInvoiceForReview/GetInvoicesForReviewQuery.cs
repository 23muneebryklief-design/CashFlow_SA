using CashFlowSA.Application.Features.InvoiceReview.Dtos;
using CashFlowSA.Domain.Models.Enums;
using MediatR;

namespace CashFlowSA.Application.Features.InvoiceReview.GetInvoicesForReview
{
    public class GetInvoicesForReviewQuery : IRequest<List<InvoiceForReviewDto>>
    {
        public InvoiceStatus? StatusFilter { get; set; }
    }
}
