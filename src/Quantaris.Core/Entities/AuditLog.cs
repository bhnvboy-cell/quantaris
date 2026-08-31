using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class AuditLog : BaseEntity, ITenantEntity
{
    public int? UserId { get; set; }
    public string? Action { get; set; }
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string? Description { get; set; }
    public string? IpAddress { get; set; }

    public User? User { get; set; }
}
