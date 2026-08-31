using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class DmaicStep : BaseEntity, ITenantEntity
{
    public int ProjectId { get; set; }
    public string? Phase { get; set; }
    public int StepNumber { get; set; }
    public string? StepName { get; set; }
    public string Status { get; set; } = "NotStarted";
    public int? OwnerId { get; set; }
    public DateTime? TargetDate { get; set; }
    public DateTime? CompletedOn { get; set; }

    public Project Project { get; set; } = null!;
    public User? Owner { get; set; }
}
