using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class Tenant : BaseEntity
{
    public string TenantCode { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
    public string? Subdomain { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsTrial { get; set; } = true;
    public DateTime? TrialEndDate { get; set; }
    public int MaxProjects { get; set; } = 10;
    public int MaxUsers { get; set; } = 50;
    public string PlanType { get; set; } = "Free";
    public string? ConnectionString { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
