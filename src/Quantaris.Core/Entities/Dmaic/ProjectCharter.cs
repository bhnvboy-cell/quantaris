using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class ProjectCharter : BaseEntity, ITenantEntity
{
    public int ProjectId { get; set; }
    public string? ProblemStatement { get; set; }
    public string? GoalStatement { get; set; }
    public string? BusinessCase { get; set; }
    public string? ScopeIncluded { get; set; }
    public string? ScopeExcluded { get; set; }
    public string? Timeline { get; set; }
    public string? ElevatorSpeech { get; set; }

    public Project Project { get; set; } = null!;
}
