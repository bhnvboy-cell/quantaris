using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class DoeExperiment : BaseEntity, ITenantEntity
{
    public int ProjectId { get; set; }
    public string ExperimentName { get; set; } = string.Empty;
    public string ResponseName { get; set; } = "Response";
    public string DesignType { get; set; } = "FullFactorial";
    public int FactorCount { get; set; } = 2;
    public int Runs { get; set; } = 4;
    public int Replicates { get; set; } = 1;
    public int CenterPoints { get; set; } = 0;
    public string? FactorsJson { get; set; }
    public string? DesignMatrixJson { get; set; }
    public string? ResponsesJson { get; set; }
    public string? AnovaJson { get; set; }
    public string? Conclusions { get; set; }

    public Project Project { get; set; } = null!;
}
