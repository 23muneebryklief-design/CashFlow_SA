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
// Provides common audit properties that are shared by all entities.
// Classes that inherit from BaseEntity automatically include information
// about when a record was created or updated and which user performed
// those actions, promoting consistency and reducing duplicate code.