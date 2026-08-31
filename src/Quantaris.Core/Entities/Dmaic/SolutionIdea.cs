using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class SolutionIdea : BaseEntity, ITenantEntity
{
    public int ProjectId { get; set; }
    public string? IdeaTitle { get; set; }
    public string? Description { get; set; }
    public string? Technique { get; set; }
    public string? LeanToolType { get; set; }
    public string? Category { get; set; }
    public decimal? SelectionScore { get; set; }
    public string? SelectionCriteria { get; set; }
    public bool IsSelected { get; set; }

    public Project Project { get; set; } = null!;
}
