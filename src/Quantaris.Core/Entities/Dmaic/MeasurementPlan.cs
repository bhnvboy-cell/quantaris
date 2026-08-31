using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class MeasurementPlan : BaseEntity, ITenantEntity
{
    public int ProjectId { get; set; }
    public string? MetricName { get; set; }
    public string? OperationalDefinition { get; set; }
    public string? DataSource { get; set; }
    public string? SamplingPlan { get; set; }
    public int? SampleSize { get; set; }
    public string? Frequency { get; set; }
    public string? Who { get; set; }
    public string? MsaType { get; set; }
    public string? MsaResult { get; set; }
    public bool? MsaAcceptable { get; set; }

    public Project Project { get; set; } = null!;
}
