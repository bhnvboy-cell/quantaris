using System.Security.Claims;
using Quantaris.Infrastructure.Data;

namespace Quantaris.Api.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var tenantIdClaim = context.User.FindFirst("TenantId");
        if (tenantIdClaim != null && int.TryParse(tenantIdClaim.Value, out var tenantId))
        {
            QuantarisDbContext.SetTenantId(tenantId);
        }

        await _next(context);
    }
}
