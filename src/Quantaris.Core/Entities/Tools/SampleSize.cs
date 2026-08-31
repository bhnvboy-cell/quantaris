using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class SampleSizeCalculation : BaseEntity, ITenantEntity
{
    public int ProjectId { get; set; }
    public string CalcName { get; set; } = string.Empty;
    public string CalcType { get; set; } = string.Empty;
    public string Parameters { get; set; } = string.Empty;
    public int Result { get; set; }

    public Project Project { get; set; } = null!;
}
