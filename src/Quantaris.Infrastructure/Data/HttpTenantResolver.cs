namespace Quantaris.Infrastructure.Data;

public class HttpTenantResolver : ITenantResolver
{
    private int _tenantId;
    private string _tenantCode = string.Empty;

    public int GetCurrentTenantId() => _tenantId;
    public string GetCurrentTenantCode() => _tenantCode;

    public void SetTenant(int tenantId, string tenantCode)
    {
        _tenantId = tenantId;
        _tenantCode = tenantCode;
    }
}
