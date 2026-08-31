using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class TrainingCourse : BaseEntity, ITenantEntity
{
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string? BeltLevel { get; set; }
    public string? Description { get; set; }
    public double? DurationHours { get; set; }
    public bool IsActive { get; set; } = true;
}

public class TrainingCertification : BaseEntity, ITenantEntity
{
    public int UserId { get; set; }
    public string CertificationName { get; set; } = string.Empty;
    public string BeltLevel { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? IssuedBy { get; set; }
    public bool IsActive { get; set; } = true;

    public User User { get; set; } = null!;
}
