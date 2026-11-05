using Quantaris.Core.Entities;
using Quantaris.Infrastructure.Data;
using Quantaris.Infrastructure.Repositories;

namespace Quantaris.Infrastructure.Services;

public class DmaicService : IDmaicService
{
    private readonly IProjectRepository _projectRepo;
    private readonly IRepository<DmaicStep> _stepRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantResolver _tenantResolver;

    public DmaicService(
        IProjectRepository projectRepo,
        IRepository<DmaicStep> stepRepo,
        IUnitOfWork unitOfWork,
        ITenantResolver tenantResolver)
    {
        _projectRepo = projectRepo;
        _stepRepo = stepRepo;
        _unitOfWork = unitOfWork;
        _tenantResolver = tenantResolver;
    }

    public async Task<IReadOnlyList<DmaicStep>> GetStepsAsync(int projectId)
        => await _projectRepo.GetDmaicStepsAsync(projectId);

    public async Task<DmaicStep?> UpdateStepStatusAsync(int projectId, int stepNumber, string status, DateTime? completedOn = null)
    {
        var tenantId = _tenantResolver.GetCurrentTenantId();
        var project = await _projectRepo.GetByIdAsync(projectId);
        if (project == null || project.TenantId != tenantId)
            return null;

        var steps = await _projectRepo.GetDmaicStepsAsync(projectId);
        var step = steps.FirstOrDefault(s => s.StepNumber == stepNumber);

        if (step == null)
        {
            step = new DmaicStep
            {
                TenantId = tenantId,
                ProjectId = projectId,
                StepNumber = stepNumber,
                StepName = GetStepName(stepNumber),
                Phase = GetPhaseForStep(stepNumber),
                Status = status,
                CompletedOn = completedOn
            };
            await _stepRepo.AddAsync(step);
        }
        else
        {
            step.Status = status;
            step.CompletedOn = completedOn;
            _stepRepo.Update(step);
        }

        await _unitOfWork.SaveChangesAsync();
        return step;
    }

    public async Task<Dictionary<string, int>> GetProgressSummaryAsync(int projectId)
    {
        var steps = await GetStepsAsync(projectId);
        return new Dictionary<string, int>
        {
            ["Define"] = steps.Count(s => s.Phase == "Define" && s.Status == "Complete"),
            ["Measure"] = steps.Count(s => s.Phase == "Measure" && s.Status == "Complete"),
            ["Analyze"] = steps.Count(s => s.Phase == "Analyze" && s.Status == "Complete"),
            ["Improve"] = steps.Count(s => s.Phase == "Improve" && s.Status == "Complete"),
            ["Control"] = steps.Count(s => s.Phase == "Control" && s.Status == "Complete")
        };
    }

    public static string GetPhaseForStep(int stepNumber) => stepNumber switch
    {
        >= 1 and <= 3 => "Define",
        >= 4 and <= 6 => "Measure",
        >= 7 and <= 9 => "Analyze",
        >= 10 and <= 12 => "Improve",
        >= 13 and <= 15 => "Control",
        _ => "Unknown"
    };

    public static string GetStepName(int stepNumber) => stepNumber switch
    {
        1 => "Understand Customer & Business Requirements",
        2 => "Complete Project Charter",
        3 => "Complete High-Level As-Is Process Map (SIPOC)",
        4 => "Identify What to Measure",
        5 => "Plan and Collect Data",
        6 => "Determine Baseline Performance",
        7 => "Identify Performance Gaps",
        8 => "Ascertain Critical Root Causes",
        9 => "Validate Root Causes",
        10 => "Generate, Prioritize and Select Solutions",
        11 => "Pilot Solution(s)",
        12 => "Validate Impact of Solutions",
        13 => "Institutionalize the Solutions",
        14 => "Replicate and Share Best Practices",
        15 => "Celebrate and Recognize Success",
        _ => "Unknown Step"
    };
}
