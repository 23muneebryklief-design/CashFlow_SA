using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Wallet.GetWalletTransactions
{
    public class GetWalletTransactionsQueryHandler : IRequestHandler<GetWalletTransactionsQuery, List<WalletTransactionDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetWalletTransactionsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<WalletTransactionDto>> Handle(GetWalletTransactionsQuery request, CancellationToken cancellationToken)
        {
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == request.UserId, cancellationToken);

            if (wallet is null)
                throw new NotFoundException("Wallet not found for this user.");

            return await _context.WalletTransactions
                .Where(t => t.WalletId == wallet.WalletId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new WalletTransactionDto
                {
                    TransactionId = t.TransactionId,
                    Type = t.Type,
                    Amount = t.Amount,
                    ReferenceType = t.ReferenceType,
                    ReferenceId = t.ReferenceId,
                    Description = t.Description,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync(cancellationToken);
        }
    }
}
