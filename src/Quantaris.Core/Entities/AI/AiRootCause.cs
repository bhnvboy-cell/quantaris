using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class AiRootCauseModel : BaseEntity, ITenantEntity
{
    public string ModelName { get; set; } = string.Empty;
    public string ModelType { get; set; } = "Bayesian";
    public bool IsActive { get; set; } = true;

    public ICollection<AiRootCausePattern> Patterns { get; set; } = new List<AiRootCausePattern>();
}

public class AiRootCausePattern
{
    public int Id { get; set; }
    public int ModelId { get; set; }
    public string SymptomKeyword { get; set; } = string.Empty;
    public string RootCause { get; set; } = string.Empty;
    public decimal Probability { get; set; } = 0.5m;
    public int OccurrenceCount { get; set; } = 1;
    public string? Category { get; set; }

    public AiRootCauseModel Model { get; set; } = null!;
}

public class AiRootCausePrediction : BaseEntity, ITenantEntity
{
    public int ProjectId { get; set; }
    public string Symptoms { get; set; } = string.Empty;
    public string? PredictedRootCause { get; set; }
    public decimal? Confidence { get; set; }
    public int? ModelId { get; set; }
    public bool? WasAccurate { get; set; }
    public string? ActualRootCause { get; set; }

    public Project? Project { get; set; }
    public AiRootCauseModel? Model { get; set; }
}
