namespace Domain.Entities
{
    /// <summary>
    /// Base entity with multitenancy support via TenantId.
    /// All entities that require tenant isolation should inherit from this class.
    /// </summary>
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public string TenantId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
