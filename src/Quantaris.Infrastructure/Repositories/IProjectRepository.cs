using Quantaris.Core.Entities;

namespace Quantaris.Infrastructure.Repositories;

public interface IProjectRepository : IRepository<Project>
{
    Task<IReadOnlyList<Project>> GetProjectsByTenantAsync(int tenantId);
    Task<Project?> GetProjectWithStepsAsync(int projectId);
    Task<IReadOnlyList<DmaicStep>> GetDmaicStepsAsync(int projectId);
}
