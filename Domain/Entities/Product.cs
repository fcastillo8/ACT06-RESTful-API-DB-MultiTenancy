namespace Domain.Entities
{
    /// <summary>
    /// Sample entity to demonstrate multitenancy data isolation.
    /// Each tenant can only see their own products.
    /// </summary>
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}
