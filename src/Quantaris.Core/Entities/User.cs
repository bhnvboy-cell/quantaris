using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class User : BaseEntity, ITenantEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Role { get; set; } = "Viewer";
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginOn { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
