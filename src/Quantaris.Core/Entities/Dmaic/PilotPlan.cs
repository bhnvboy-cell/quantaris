using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class PilotPlan : BaseEntity, ITenantEntity
{
    public int ProjectId { get; set; }
    public string? PlanName { get; set; }
    public string? Scope { get; set; }
    public string? Duration { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Metrics { get; set; }
    public string? PokaYokeIdeas { get; set; }
    public string? ImplementationPlan { get; set; }
    public string? Status { get; set; }

    public Project Project { get; set; } = null!;
}
