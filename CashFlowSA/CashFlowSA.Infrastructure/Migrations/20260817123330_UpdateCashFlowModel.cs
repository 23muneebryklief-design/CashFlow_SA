using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CashFlowSA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCashFlowModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewNotes",
                table: "Invoices",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "Invoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewedByUserId",
                table: "Invoices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ReviewedByUserId",
                table: "Invoices",
                column: "ReviewedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Invoices_ReviewedByUserId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ReviewNotes",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                table: "Invoices");
        }
    }
}
