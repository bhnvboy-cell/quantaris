using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class BestPractice : BaseEntity, ITenantEntity
{
    public int ProjectId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? ApplicableAreas { get; set; }
    public string? Sites { get; set; }
    public string? Teams { get; set; }
    public bool IsReplicated { get; set; }
    public DateTime? ReplicatedOn { get; set; }

    public Project Project { get; set; } = null!;
}
