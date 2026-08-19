using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CashFlowSA.Infrastructure.Migrations
{
    public partial class HardenAuditLogsAppendOnly : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[TR_AuditLogs_AppendOnly]', N'TR') IS NOT NULL
    DROP TRIGGER [dbo].[TR_AuditLogs_AppendOnly];

EXEC(N'
CREATE TRIGGER [dbo].[TR_AuditLogs_AppendOnly]
ON [dbo].[AuditLogs]
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    THROW 51000, ''Audit logs are append-only and cannot be updated or deleted.'', 1;
END
');");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[TR_AuditLogs_AppendOnly]', N'TR') IS NOT NULL
    DROP TRIGGER [dbo].[TR_AuditLogs_AppendOnly];");
        }
    }
}
