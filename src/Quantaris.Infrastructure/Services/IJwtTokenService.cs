using Quantaris.Core.Entities;

namespace Quantaris.Infrastructure.Services;

public interface IJwtTokenService
{
    string GenerateToken(User user, Tenant tenant);
}
