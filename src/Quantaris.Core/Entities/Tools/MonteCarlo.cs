using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class MonteCarloModel : BaseEntity, ITenantEntity
{
    public int ProjectId { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public int Iterations { get; set; } = 10000;
    public string? OutputFormula { get; set; }

    public Project Project { get; set; } = null!;
    public ICollection<MonteCarloInput> Inputs { get; set; } = new List<MonteCarloInput>();
    public ICollection<MonteCarloResult> Results { get; set; } = new List<MonteCarloResult>();
}

public class MonteCarloInput
{
    public int Id { get; set; }
    public int ModelId { get; set; }
    public string InputName { get; set; } = string.Empty;
    public string DistributionType { get; set; } = string.Empty;
    public double Param1 { get; set; }
    public double? Param2 { get; set; }
    public double? Param3 { get; set; }

    public MonteCarloModel Model { get; set; } = null!;
}

public class MonteCarloResult
{
    public int Id { get; set; }
    public int ModelId { get; set; }
    public int ResultIndex { get; set; }
    public double ResultValue { get; set; }

    public MonteCarloModel Model { get; set; } = null!;
}
