namespace Application.Interfaces
{
    /// <summary>
    /// Service to resolve the current tenant from the authenticated user's JWT claims.
    /// </summary>
    public interface ITenantService
    {
        string GetCurrentTenantId();
    }
}
