using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class GanttTask : BaseEntity, ITenantEntity
{
    public int ProjectId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Phase { get; set; } = "Define";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Progress { get; set; } = 0;
    public int? DependencyId { get; set; }
    public int? AssignedToUserId { get; set; }
    public string? Priority { get; set; } = "Medium";
    public string? Status { get; set; } = "Not Started";
    public int SortOrder { get; set; } = 0;

    public Project Project { get; set; } = null!;
    public GanttTask? Dependency { get; set; }
    public User? AssignedTo { get; set; }
}

public class GanttMilestone : BaseEntity, ITenantEntity
{
    public int ProjectId { get; set; }
    public string MilestoneName { get; set; } = string.Empty;
    public DateTime MilestoneDate { get; set; }
    public string? Phase { get; set; }
    public bool IsCompleted { get; set; }
    public int SortOrder { get; set; } = 0;

    public Project Project { get; set; } = null!;
}
