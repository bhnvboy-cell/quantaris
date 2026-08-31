namespace Quantaris.Infrastructure.Data;

public interface ITenantResolver
{
    int GetCurrentTenantId();
    string GetCurrentTenantCode();
    void SetTenant(int tenantId, string tenantCode);
}
