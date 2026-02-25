using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Security
{
    /// <summary>
    /// Resolves the current tenant from the JWT token claims.
    /// Returns "default" if no tenant claim is found (e.g., unauthenticated requests).
    /// </summary>
    public class TenantService : ITenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCurrentTenantId()
        {
            var tenantId = _httpContextAccessor.HttpContext?.User?.FindFirstValue("tenantId");
            return tenantId ?? "default";
        }
    }
}
