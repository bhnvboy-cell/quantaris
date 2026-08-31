using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class VocRecord : BaseEntity, ITenantEntity
{
    public int ProjectId { get; set; }
    public string? CustomerName { get; set; }
    public string? VoiceStatement { get; set; }
    public string? AffinityGrouping { get; set; }
    public string? TranslatedNeed { get; set; }
    public string? Requirement { get; set; }
    public string? KanoClassification { get; set; }

    public Project Project { get; set; } = null!;
}
