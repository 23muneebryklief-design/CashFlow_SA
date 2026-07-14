namespace CashFlowSA.Domain.Models
{
    public abstract class BaseEntity
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public Guid? CreatedByUserId { get; set; }

        public Guid? UpdatedByUserId { get; set; }
    }
}
