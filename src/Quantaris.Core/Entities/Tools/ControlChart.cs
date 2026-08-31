using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class ControlChartModel : BaseEntity, ITenantEntity
{
    public int ProjectId { get; set; }
    public string ChartName { get; set; } = string.Empty;
    public string ChartType { get; set; } = string.Empty;
    public int SubgroupSize { get; set; } = 1;

    public Project Project { get; set; } = null!;
    public ICollection<ControlChartData> DataPoints { get; set; } = new List<ControlChartData>();
    public ControlChartLimits? Limits { get; set; }
}

public class ControlChartData
{
    public int Id { get; set; }
    public int ModelId { get; set; }
    public int SampleIndex { get; set; }
    public string SampleValues { get; set; } = string.Empty;

    public ControlChartModel Model { get; set; } = null!;
}

public class ControlChartLimits
{
    public int Id { get; set; }
    public int ModelId { get; set; }
    public double CenterLine { get; set; }
    public double UCL { get; set; }
    public double LCL { get; set; }
    public double? CenterLineR { get; set; }
    public double? UCLR { get; set; }
    public double? LCLR { get; set; }

    public ControlChartModel Model { get; set; } = null!;
}
