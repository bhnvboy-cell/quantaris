using Quantaris.Infrastructure.Data;

namespace Quantaris.Api.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ITenantResolver tenantResolver)
    {
        var tenantIdClaim = context.User.FindFirst("TenantId");
        var tenantCodeClaim = context.User.FindFirst("TenantCode");
        if (tenantIdClaim != null && int.TryParse(tenantIdClaim.Value, out var tenantId))
        {
            tenantResolver.SetTenant(tenantId, tenantCodeClaim?.Value ?? string.Empty);
        }

        await _next(context);
    }
}
