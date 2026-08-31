using Microsoft.EntityFrameworkCore;
using Quantaris.Core.Entities;
using Quantaris.Infrastructure.Data;

namespace Quantaris.Infrastructure.Repositories;

public class ProjectRepository : Repository<Project>, IProjectRepository
{
    public ProjectRepository(QuantarisDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Project>> GetProjectsByTenantAsync(int tenantId)
        => await _dbSet.Where(p => p.TenantId == tenantId)
            .OrderByDescending(p => p.CreatedOn)
            .ToListAsync();

    public async Task<Project?> GetProjectWithStepsAsync(int projectId)
        => await _dbSet
            .Include(p => p.DmaicSteps)
            .FirstOrDefaultAsync(p => p.Id == projectId);

    public async Task<IReadOnlyList<DmaicStep>> GetDmaicStepsAsync(int projectId)
        => await _context.DmaicSteps
            .Where(s => s.ProjectId == projectId)
            .OrderBy(s => s.StepNumber)
            .ToListAsync();
}
