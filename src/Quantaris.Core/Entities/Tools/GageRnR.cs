using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class GageRnRStudy : BaseEntity, ITenantEntity
{
    public int ProjectId { get; set; }
    public string StudyName { get; set; } = string.Empty;
    public int PartCount { get; set; } = 5;
    public int AppraiserCount { get; set; } = 2;
    public int TrialCount { get; set; } = 2;
    public string? PartNames { get; set; }
    public string? AppraiserNames { get; set; }
    public string? MeasurementsJson { get; set; }
    public string? ResultsJson { get; set; }
    public string? Conclusion { get; set; }
    public string? Notes { get; set; }

    public Project Project { get; set; } = null!;
}
