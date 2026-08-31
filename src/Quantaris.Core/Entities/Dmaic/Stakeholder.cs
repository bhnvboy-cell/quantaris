using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class Stakeholder : BaseEntity, ITenantEntity
{
    public int ProjectId { get; set; }
    public string? Name { get; set; }
    public string? Role { get; set; }
    public string? Interest { get; set; }
    public string? Influence { get; set; }
    public string? ArmiType { get; set; }
    public string? RaciType { get; set; }
    public string? Notes { get; set; }

    public Project Project { get; set; } = null!;
}
