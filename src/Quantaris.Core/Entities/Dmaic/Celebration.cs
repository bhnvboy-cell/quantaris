using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class Celebration : BaseEntity, ITenantEntity
{
    public int ProjectId { get; set; }
    public string? CelebrationType { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Participants { get; set; }
    public DateTime? RecognitionDate { get; set; }

    public Project Project { get; set; } = null!;
}
