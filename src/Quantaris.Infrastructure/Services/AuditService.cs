using Quantaris.Core.Entities;
using Quantaris.Infrastructure.Data;
using Quantaris.Infrastructure.Repositories;

namespace Quantaris.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly IRepository<AuditLog> _auditRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantResolver _tenantResolver;

    public AuditService(IRepository<AuditLog> auditRepo, IUnitOfWork unitOfWork, ITenantResolver tenantResolver)
    {
        _auditRepo = auditRepo;
        _unitOfWork = unitOfWork;
        _tenantResolver = tenantResolver;
    }

    public async Task LogAsync(string action, string entityType, int? entityId, string description, string? ipAddress = null)
    {
        var tenantId = _tenantResolver.GetCurrentTenantId();
        var log = new AuditLog
        {
            TenantId = tenantId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Description = description,
            IpAddress = ipAddress,
            CreatedOn = DateTime.UtcNow
        };
        await _auditRepo.AddAsync(log);
        await _unitOfWork.SaveChangesAsync();
    }
}
