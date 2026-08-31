using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class FmeaRecord : BaseEntity, ITenantEntity
{
    public int ProjectId { get; set; }
    public string? ProcessStep { get; set; }
    public string? FailureMode { get; set; }
    public string? EffectOfFailure { get; set; }
    public string? CauseOfFailure { get; set; }
    public string? CurrentControl { get; set; }
    public int Severity { get; set; }
    public int Occurrence { get; set; }
    public int Detection { get; set; }
    public int Rpn { get; set; }
    public string? RecommendedAction { get; set; }
    public string? ActionTaken { get; set; }
    public int? NewSeverity { get; set; }
    public int? NewOccurrence { get; set; }
    public int? NewDetection { get; set; }
    public int? NewRpn { get; set; }

    public Project Project { get; set; } = null!;
}
