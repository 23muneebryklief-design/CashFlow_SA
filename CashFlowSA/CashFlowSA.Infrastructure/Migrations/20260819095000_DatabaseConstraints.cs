using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CashFlowSA.Infrastructure.Migrations
{
    /// <summary>
    /// Priority 12: database-level financial and business integrity constraints.
    /// </summary>
    public partial class Priority12DatabaseConstraints : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Wallet_Balance_NonNegative",
                table: "Wallets",
                sql: "[Balance] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_WalletTransaction_Amount_Positive",
                table: "WalletTransactions",
                sql: "[Amount] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FundingRequest_RequestedAmount_Positive",
                table: "FundingRequests",
                sql: "[RequestedAmount] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FundingCampaign_Amounts_Valid",
                table: "FundingCampaigns",
                sql: "[TargetAmount] > 0 AND [FundedAmount] >= 0 AND [FundedAmount] <= [TargetAmount]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FundingCampaign_ReturnRate_Valid",
                table: "FundingCampaigns",
                sql: "[ExpectedReturnRate] IS NULL OR ([ExpectedReturnRate] >= 0 AND [ExpectedReturnRate] <= 100)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AuctionBid_Amount_Positive",
                table: "AuctionBids",
                sql: "[BidAmount] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AuctionBid_ReturnRate_Valid",
                table: "AuctionBids",
                sql: "[ProposedReturnRate] >= 0 AND [ProposedReturnRate] <= 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Investment_Amount_Positive",
                table: "Investments",
                sql: "[Amount] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Investment_ReturnAmount_NonNegative",
                table: "Investments",
                sql: "[ReturnAmount] IS NULL OR [ReturnAmount] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Settlement_SettledAmount_Positive",
                table: "Settlements",
                sql: "[SettledAmount] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReturnDistribution_Amounts_NonNegative",
                table: "ReturnDistributions",
                sql: "[PrincipalAmount] >= 0 AND [ReturnAmount] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Invoice_Amount_Positive",
                table: "Invoices",
                sql: "[Amount] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Invoice_DueDate_NotBeforeIssueDate",
                table: "Invoices",
                sql: "[DueDate] >= [IssueDate]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OCRResult_ConfidenceScore_Valid",
                table: "OCRResults",
                sql: "[ConfidenceScore] >= 0 AND [ConfidenceScore] <= 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OCRResult_ExtractedAmount_NonNegative",
                table: "OCRResults",
                sql: "[ExtractedAmount] IS NULL OR [ExtractedAmount] >= 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(name: "CK_Wallet_Balance_NonNegative", table: "Wallets");
            migrationBuilder.DropCheckConstraint(name: "CK_WalletTransaction_Amount_Positive", table: "WalletTransactions");
            migrationBuilder.DropCheckConstraint(name: "CK_FundingRequest_RequestedAmount_Positive", table: "FundingRequests");
            migrationBuilder.DropCheckConstraint(name: "CK_FundingCampaign_Amounts_Valid", table: "FundingCampaigns");
            migrationBuilder.DropCheckConstraint(name: "CK_FundingCampaign_ReturnRate_Valid", table: "FundingCampaigns");
            migrationBuilder.DropCheckConstraint(name: "CK_AuctionBid_Amount_Positive", table: "AuctionBids");
            migrationBuilder.DropCheckConstraint(name: "CK_AuctionBid_ReturnRate_Valid", table: "AuctionBids");
            migrationBuilder.DropCheckConstraint(name: "CK_Investment_Amount_Positive", table: "Investments");
            migrationBuilder.DropCheckConstraint(name: "CK_Investment_ReturnAmount_NonNegative", table: "Investments");
            migrationBuilder.DropCheckConstraint(name: "CK_Settlement_SettledAmount_Positive", table: "Settlements");
            migrationBuilder.DropCheckConstraint(name: "CK_ReturnDistribution_Amounts_NonNegative", table: "ReturnDistributions");
            migrationBuilder.DropCheckConstraint(name: "CK_Invoice_Amount_Positive", table: "Invoices");
            migrationBuilder.DropCheckConstraint(name: "CK_Invoice_DueDate_NotBeforeIssueDate", table: "Invoices");
            migrationBuilder.DropCheckConstraint(name: "CK_OCRResult_ConfidenceScore_Valid", table: "OCRResults");
            migrationBuilder.DropCheckConstraint(name: "CK_OCRResult_ExtractedAmount_NonNegative", table: "OCRResults");
        }
    }
}
