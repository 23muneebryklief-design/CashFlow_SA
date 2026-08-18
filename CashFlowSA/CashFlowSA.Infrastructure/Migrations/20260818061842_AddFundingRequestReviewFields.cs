using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CashFlowSA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFundingRequestReviewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewNotes",
                table: "FundingRequests",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewerId",
                table: "FundingRequests",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewNotes",
                table: "FundingRequests");

            migrationBuilder.DropColumn(
                name: "ReviewerId",
                table: "FundingRequests");
        }
    }
}
