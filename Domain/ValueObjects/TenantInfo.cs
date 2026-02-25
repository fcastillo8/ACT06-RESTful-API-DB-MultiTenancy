namespace Domain.ValueObjects
{
    /// <summary>
    /// Value object representing tenant context information.
    /// Extracted from the JWT token claims.
    /// </summary>
    public class TenantInfo
    {
        public string TenantId { get; }
        public string TenantName { get; }

        public TenantInfo(string tenantId, string tenantName = "")
        {
            if (string.IsNullOrWhiteSpace(tenantId))
                throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));

            TenantId = tenantId;
            TenantName = tenantName;
        }
    }
}
