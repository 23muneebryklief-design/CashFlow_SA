namespace CashFlowSA.Domain.Models.Enums
{
    public enum AuditAction
    {
        Created=0,
        Updated=1,
        Deleted=2,

        Submitted=3,
        Approved=4,
        Rejected=5,

        UploadedDocument=6,
        DownloadedDocument=7,

        UploadedInvoice=8,

        Invested=9,
        Funded=10,
        Settled=11,

        LoggedIn=12,
        LoggedOut=13,
        RiskOverridden=14,
        UserSuspended=15,
        UserReinstated=16,
        AuditReportGenerated=17,
    }
}