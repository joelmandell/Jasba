using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SBAPro.Core.Interfaces;

namespace SBAPro.Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetTenantId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || !user.Identity?.IsAuthenticated == true)
        {
            return Guid.Empty;
        }

        var tenantIdClaim = user.FindFirst("TenantId");
        if (tenantIdClaim != null && Guid.TryParse(tenantIdClaim.Value, out var tenantId))
        {
            return tenantId;
        }

        return Guid.Empty;
    }
}
