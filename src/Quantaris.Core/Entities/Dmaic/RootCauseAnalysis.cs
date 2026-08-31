using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class RootCauseAnalysis : BaseEntity, ITenantEntity
{
    public int ProjectId { get; set; }
    public string? Category { get; set; }
    public string? Cause { get; set; }
    public string? Effect { get; set; }
    public bool IsFishbone { get; set; }
    public string? FiveWhyChain { get; set; }
    public int? Sequence { get; set; }

    public Project Project { get; set; } = null!;
}
