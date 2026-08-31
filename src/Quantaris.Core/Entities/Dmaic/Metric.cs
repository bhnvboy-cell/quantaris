using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class Metric : BaseEntity, ITenantEntity
{
    public int ProjectId { get; set; }
    public int? MeasurementPlanId { get; set; }
    public string? MetricName { get; set; }
    public string? DataType { get; set; }
    public decimal? Baseline { get; set; }
    public decimal? Target { get; set; }
    public decimal? CurrentValue { get; set; }
    public decimal? SigmaLevel { get; set; }
    public decimal? Dpmo { get; set; }
    public decimal? CapabilityCp { get; set; }
    public decimal? CapabilityCpk { get; set; }
    public string? NormalityTestResult { get; set; }

    public Project Project { get; set; } = null!;
}
