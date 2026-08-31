using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class CtqRequirement : BaseEntity, ITenantEntity
{
    public int ProjectId { get; set; }
    public string? CtqName { get; set; }
    public string? Category { get; set; }
    public string? Requirement { get; set; }
    public string? Specification { get; set; }
    public string? TargetValue { get; set; }
    public string? UpperLimit { get; set; }
    public string? LowerLimit { get; set; }
    public bool IsCritical { get; set; }

    public Project Project { get; set; } = null!;
}
