using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class MetricsHistory : BaseEntity, ITenantEntity
{
    public int ProjectId { get; set; }
    public int? MetricId { get; set; }
    public string? MetricName { get; set; }
    public decimal? Value { get; set; }
    public DateTime RecordedOn { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public Metric? Metric { get; set; }
}
