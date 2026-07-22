namespace CashFlowSA.Domain.Models.Enums
{
    public enum NotificationEvent
    {
        // Account
        AccountCreated=0,
        AccountVerified=1,
        PasswordReset=2,

        // KYC
        KycSubmitted=3,
        KycApproved=4,
        KycRejected=5,
        KycRequiresAdditionalInformation=6,

        // Documents
        DocumentUploaded=7,
        DocumentApproved=8,
        DocumentRejected=9,

        // Invoices
        InvoiceUploaded=10,
        InvoiceApproved=11,
        InvoiceRejected=12,
        InvoiceDueSoon=13,

        // Funding Requests
        FundingRequestSubmitted=14,
        FundingRequestApproved=15,
        FundingRequestRejected=16,

        // Marketplace
        ListingPublished=17,
        ListingClosed=18,

        // Investments
        InvestmentReceived=19,
        InvestmentConfirmed=20,

        // Campaigns
        CampaignFunded=21,
        CampaignExpired=22,

        // Settlements
        SettlementCompleted=23,
        ReturnsDistributed=24,

        // Risk Assessment
        RiskAssessmentCompleted=25,
        RiskScoreOverridden=26,

        // Administrative
        AdminAlert=27,
        SystemAnnouncement=28
    }
}