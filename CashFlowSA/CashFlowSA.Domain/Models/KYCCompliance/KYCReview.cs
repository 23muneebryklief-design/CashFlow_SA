using CashFlowSA.Domain.Models.Enums;
namespace CashFlowSA.Domain.Models
{
    public class KYCReview
    {
        public Guid Id { get; set; }

        public Guid KYCApplicationId { get; set; }

        public Guid ReviewerId { get; set; } // User ID of Credit Analyst

        public ReviewOutcome Outcome { get; set; }

        public string? Notes { get; set; }

        public DateTime ReviewDate { get; set; } = DateTime.UtcNow;
    }
}