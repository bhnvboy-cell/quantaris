namespace Quantaris.Infrastructure.Services;

public interface IAuditService
{
    Task LogAsync(string action, string entityType, int? entityId, string description, string? ipAddress = null);
}
