using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class RegressionModel : BaseEntity, ITenantEntity
{
    public int ProjectId { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public string ResponseName { get; set; } = string.Empty;
    public string PredictorNames { get; set; } = string.Empty;

    public Project Project { get; set; } = null!;
    public ICollection<RegressionData> DataPoints { get; set; } = new List<RegressionData>();
}

public class RegressionData
{
    public int Id { get; set; }
    public int ModelId { get; set; }
    public int DataIndex { get; set; }
    public double ResponseValue { get; set; }
    public string PredictorValues { get; set; } = string.Empty;

    public RegressionModel Model { get; set; } = null!;
}
