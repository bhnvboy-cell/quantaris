namespace Quantaris.Core.Entities.Common;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; set; }
}
