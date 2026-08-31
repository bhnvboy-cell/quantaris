using Quantaris.Core.Entities;

namespace Quantaris.Infrastructure.Services;

public interface IDmaicService
{
    Task<IReadOnlyList<DmaicStep>> GetStepsAsync(int projectId);
    Task<DmaicStep?> UpdateStepStatusAsync(int projectId, int stepNumber, string status, DateTime? completedOn = null);
    Task<Dictionary<string, int>> GetProgressSummaryAsync(int projectId);
}
