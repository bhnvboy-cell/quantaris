using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class BenefitsTracking : BaseEntity, ITenantEntity
{
    public int ProjectId { get; set; }
    public string BenefitName { get; set; } = string.Empty;
    public string BenefitType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime StartMonth { get; set; }
    public DateTime EndMonth { get; set; }
    public string Status { get; set; } = "Planned";
    public string? Notes { get; set; }

    public Project Project { get; set; } = null!;
    public ICollection<BenefitsActual> Actuals { get; set; } = new List<BenefitsActual>();
}

public class BenefitsActual
{
    public int Id { get; set; }
    public int BenefitId { get; set; }
    public DateTime Month { get; set; }
    public decimal ActualAmount { get; set; }
    public string? Notes { get; set; }

    public BenefitsTracking Benefit { get; set; } = null!;
}
