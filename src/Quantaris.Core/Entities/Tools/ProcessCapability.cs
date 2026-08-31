using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class ProcessCapabilityRecord : BaseEntity, ITenantEntity
{
    public int ProjectId { get; set; }
    public string? MetricName { get; set; }
    public decimal? UsL { get; set; }
    public decimal? LsL { get; set; }
    public decimal? Target { get; set; }
    public decimal? Mean { get; set; }
    public decimal? StdDev { get; set; }
    public decimal? Cp { get; set; }
    public decimal? Cpk { get; set; }
    public decimal? Ppk { get; set; }
    public decimal? SigmaLevel { get; set; }
    public decimal? Dpmo { get; set; }
    public int? SampleSize { get; set; }
    public string? DataPoints { get; set; }

    public Project Project { get; set; } = null!;
}
