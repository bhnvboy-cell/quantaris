using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class ControlPlan : BaseEntity, ITenantEntity
{
    public int ProjectId { get; set; }
    public string? ProcessStep { get; set; }
    public string? ControlMethod { get; set; }
    public string? Specification { get; set; }
    public int? SampleSize { get; set; }
    public string? Frequency { get; set; }
    public string? ReactionPlan { get; set; }
    public string? Responsible { get; set; }

    public Project Project { get; set; } = null!;
}
