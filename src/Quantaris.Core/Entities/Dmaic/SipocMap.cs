using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class SipocMap : BaseEntity, ITenantEntity
{
    public int ProjectId { get; set; }
    public string? Suppliers { get; set; }
    public string? Inputs { get; set; }
    public string? ProcessSteps { get; set; }
    public string? Outputs { get; set; }
    public string? Customers { get; set; }

    public Project Project { get; set; } = null!;
}
