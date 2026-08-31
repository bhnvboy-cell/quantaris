using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class HoshinKanriModel : BaseEntity, ITenantEntity
{
    public int ProjectId { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public int FiscalYear { get; set; }

    public Project Project { get; set; } = null!;
    public ICollection<HoshinKanriItem> Items { get; set; } = new List<HoshinKanriItem>();
}

public class HoshinKanriItem
{
    public int Id { get; set; }
    public int ModelId { get; set; }
    public string ItemType { get; set; } = string.Empty;
    public int ItemIndex { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? Owner { get; set; }
    public DateTime? TargetDate { get; set; }
    public string? Metrics { get; set; }
    public double Progress { get; set; }
    public string Status { get; set; } = "NotStarted";
    public int? ParentId { get; set; }

    public HoshinKanriModel Model { get; set; } = null!;
    public HoshinKanriItem? Parent { get; set; }
    public ICollection<HoshinKanriItem> Children { get; set; } = new List<HoshinKanriItem>();
}
