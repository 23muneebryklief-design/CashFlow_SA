using CashFlowSA.Models.enums;
namespace CashFlowSA.Models.KYCCompliance;
public class KYCReview
{
    public Guid Id { get; set; }

    public Guid KYCApplicationId { get; set; }

    public Guid ReviewerId { get; set; } // User ID of Credit Analyst

    public ReviewOutcome Outcome { get; set; }

    public string? Notes { get; set; }

    public DateTime ReviewDate { get; set; } = DateTime.UtcNow;
}
