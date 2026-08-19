using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Domain.Models;
using CashFlowSA.Domain.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Common;

public static class InvestorWalletDebit
{
    public static async Task DebitAsync(
        IApplicationDbContext context,
        Guid investorId,
        decimal amount,
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        if (amount <= 0)
            throw new ConflictException("Investment amount must be greater than zero.");

        var investor = await context.Investors
            .FirstOrDefaultAsync(i => i.InvestorId == investorId, cancellationToken);

        if (investor is null)
            throw new NotFoundException("Investor profile not found.");

        var wallet = await context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == investor.UserId, cancellationToken);

        if (wallet is null)
            throw new NotFoundException("Investor wallet not found.");

        if (wallet.Balance < amount)
            throw new ConflictException("Insufficient wallet balance for this investment.");

        wallet.Balance -= amount;

        context.WalletTransactions.Add(new WalletTransaction
        {
            TransactionId = Guid.NewGuid(),
            WalletId = wallet.WalletId,
            Type = WalletTransactionType.Debit,
            Amount = amount,
            ReferenceType = "FundingCampaign",
            ReferenceId = campaignId,
            Description = $"Investment commitment for campaign {campaignId}"
        });

        var portfolio = await context.InvestorPortfolios
            .FirstOrDefaultAsync(p => p.InvestorId == investorId, cancellationToken);

        if (portfolio is null)
        {
            portfolio = new InvestorPortfolio
            {
                PortfolioId = Guid.NewGuid(),
                InvestorId = investorId
            };
            context.InvestorPortfolios.Add(portfolio);
        }

        portfolio.TotalCommitted += amount;
        portfolio.ActiveInvestmentsCount += 1;
    }
}
