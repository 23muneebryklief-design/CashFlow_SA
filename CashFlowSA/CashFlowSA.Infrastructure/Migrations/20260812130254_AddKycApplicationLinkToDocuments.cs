using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CashFlowSA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKycApplicationLinkToDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "KYCApplicationId",
                table: "KYCDocuments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KYCDocuments_KYCApplicationId",
                table: "KYCDocuments",
                column: "KYCApplicationId");

            migrationBuilder.AddForeignKey(
                name: "FK_KYCDocuments_KYCApplications_KYCApplicationId",
                table: "KYCDocuments",
                column: "KYCApplicationId",
                principalTable: "KYCApplications",
                principalColumn: "ApplicationId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KYCDocuments_KYCApplications_KYCApplicationId",
                table: "KYCDocuments");

            migrationBuilder.DropIndex(
                name: "IX_KYCDocuments_KYCApplicationId",
                table: "KYCDocuments");

            migrationBuilder.DropColumn(
                name: "KYCApplicationId",
                table: "KYCDocuments");
        }
    }
}
