using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CashFlowSA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKycDocumentReviewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewNotes",
                table: "KYCDocuments",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "KYCDocuments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewedByUserId",
                table: "KYCDocuments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KYCDocuments_ReviewedByUserId",
                table: "KYCDocuments",
                column: "ReviewedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_KYCDocuments_ReviewedByUserId",
                table: "KYCDocuments");

            migrationBuilder.DropColumn(
                name: "ReviewNotes",
                table: "KYCDocuments");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "KYCDocuments");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                table: "KYCDocuments");
        }
    }
}
