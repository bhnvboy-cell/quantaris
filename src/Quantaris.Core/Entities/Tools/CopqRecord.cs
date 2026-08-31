using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class CopqRecord : BaseEntity, ITenantEntity
{
    public int ProjectId { get; set; }
    public DateTime EntryDate { get; set; }
    public decimal AppraisalCosts { get; set; }
    public decimal PreventionCosts { get; set; }
    public decimal InternalFailureCosts { get; set; }
    public decimal ExternalFailureCosts { get; set; }
    public decimal? TotalCopq { get; set; }
    public decimal? Revenue { get; set; }
    public string? Notes { get; set; }

    public Project Project { get; set; } = null!;
}
