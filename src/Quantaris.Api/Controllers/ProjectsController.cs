using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quantaris.Core.Entities;
using Quantaris.Infrastructure.Data;
using Quantaris.Infrastructure.Repositories;
using Quantaris.Infrastructure.Services;

namespace Quantaris.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectRepository _projectRepo;
    private readonly IRepository<FmeaRecord> _fmeaRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _audit;
    private readonly QuantarisDbContext _db;

    public ProjectsController(
        IProjectRepository projectRepo,
        IRepository<FmeaRecord> fmeaRepo,
        IUnitOfWork unitOfWork,
        IAuditService audit,
        QuantarisDbContext db)
    {
        _projectRepo = projectRepo;
        _fmeaRepo = fmeaRepo;
        _unitOfWork = unitOfWork;
        _audit = audit;
        _db = db;
    }

    private int GetTenantId() => QuantarisDbContext.GetCurrentTenantId();

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var projects = await _projectRepo.GetProjectsByTenantAsync(GetTenantId());
        return Ok(projects.Select(p => new
        {
            p.Id, p.ProjectCode, p.ProjectName, p.Description,
            p.Status, p.Phase, p.Priority, p.FinancialImpact,
            p.StartDate, p.TargetEndDate, p.ActualEndDate,
            p.Champion, p.CreatedOn
        }));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var project = await _projectRepo.GetProjectWithStepsAsync(id);
        if (project == null || project.TenantId != GetTenantId()) return NotFound();
        return Ok(project);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest request)
    {
        var tenantId = GetTenantId();
        if (await _db.Projects.AnyAsync(p => p.TenantId == tenantId && p.ProjectCode == request.ProjectCode))
            return Conflict(new { error = "Project code already exists" });

        var project = new Project
        {
            TenantId = tenantId,
            ProjectCode = request.ProjectCode,
            ProjectName = request.ProjectName,
            Description = request.Description,
            Priority = request.Priority,
            FinancialImpact = request.FinancialImpact,
            StartDate = request.StartDate,
            TargetEndDate = request.TargetEndDate,
            Champion = request.Champion,
            Status = "NotStarted",
            Phase = "Define",
            CreatedOn = DateTime.UtcNow
        };

        await _projectRepo.AddAsync(project);
        await _unitOfWork.SaveChangesAsync();
        await _audit.LogAsync("Create", "Project", project.Id, $"Created project {project.ProjectCode}");

        return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProjectRequest request)
    {
        var project = await _projectRepo.GetByIdAsync(id);
        if (project == null || project.TenantId != GetTenantId()) return NotFound();

        project.ProjectName = request.ProjectName ?? project.ProjectName;
        project.Description = request.Description ?? project.Description;
        project.Status = request.Status ?? project.Status;
        project.Phase = request.Phase ?? project.Phase;
        project.Priority = request.Priority ?? project.Priority;
        project.FinancialImpact = request.FinancialImpact ?? project.FinancialImpact;
        project.TargetEndDate = request.TargetEndDate ?? project.TargetEndDate;
        project.Champion = request.Champion ?? project.Champion;

        _projectRepo.Update(project);
        await _unitOfWork.SaveChangesAsync();
        await _audit.LogAsync("Update", "Project", project.Id, $"Updated project {project.ProjectCode}");

        return Ok(project);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var project = await _projectRepo.GetByIdAsync(id);
        if (project == null || project.TenantId != GetTenantId()) return NotFound();

        _projectRepo.Delete(project);
        await _unitOfWork.SaveChangesAsync();
        await _audit.LogAsync("Delete", "Project", project.Id, $"Deleted project {project.ProjectCode}");

        return NoContent();
    }

    // --- FMEA CRUD ---
    [HttpGet("{projectId}/fmea")]
    public async Task<IActionResult> GetFmea(int projectId)
    {
        var records = await _fmeaRepo.FindAsync(f => f.ProjectId == projectId && f.TenantId == GetTenantId());
        return Ok(records);
    }

    [HttpPost("{projectId}/fmea")]
    public async Task<IActionResult> CreateFmea(int projectId, [FromBody] FmeaRecord record)
    {
        record.TenantId = GetTenantId();
        record.ProjectId = projectId;
        record.Rpn = record.Severity * record.Occurrence * record.Detection;
        record.NewRpn = record.NewSeverity.HasValue && record.NewOccurrence.HasValue && record.NewDetection.HasValue
            ? record.NewSeverity.Value * record.NewOccurrence.Value * record.NewDetection.Value
            : null;
        await _fmeaRepo.AddAsync(record);
        await _unitOfWork.SaveChangesAsync();
        return CreatedAtAction(nameof(GetFmea), new { projectId }, record);
    }

    [HttpPut("{projectId}/fmea/{id}")]
    public async Task<IActionResult> UpdateFmea(int projectId, int id, [FromBody] FmeaRecord record)
    {
        var existing = (await _fmeaRepo.FindAsync(f => f.Id == id && f.ProjectId == projectId)).FirstOrDefault();
        if (existing == null) return NotFound();

        existing.ProcessStep = record.ProcessStep;
        existing.FailureMode = record.FailureMode;
        existing.EffectOfFailure = record.EffectOfFailure;
        existing.CauseOfFailure = record.CauseOfFailure;
        existing.CurrentControl = record.CurrentControl;
        existing.Severity = record.Severity;
        existing.Occurrence = record.Occurrence;
        existing.Detection = record.Detection;
        existing.Rpn = record.Severity * record.Occurrence * record.Detection;
        existing.RecommendedAction = record.RecommendedAction;
        existing.ActionTaken = record.ActionTaken;
        existing.NewSeverity = record.NewSeverity;
        existing.NewOccurrence = record.NewOccurrence;
        existing.NewDetection = record.NewDetection;
        existing.NewRpn = record.NewSeverity.HasValue && record.NewOccurrence.HasValue && record.NewDetection.HasValue
            ? record.NewSeverity.Value * record.NewOccurrence.Value * record.NewDetection.Value
            : null;

        _fmeaRepo.Update(existing);
        await _unitOfWork.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete("{projectId}/fmea/{id}")]
    public async Task<IActionResult> DeleteFmea(int projectId, int id)
    {
        var existing = (await _fmeaRepo.FindAsync(f => f.Id == id && f.ProjectId == projectId)).FirstOrDefault();
        if (existing == null) return NotFound();
        _fmeaRepo.Delete(existing);
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }
}

public record CreateProjectRequest(string ProjectCode, string ProjectName, string? Description,
    string? Priority, decimal? FinancialImpact, DateTime? StartDate, DateTime? TargetEndDate, string? Champion);
public record UpdateProjectRequest(string? ProjectName, string? Description, string? Status, string? Phase,
    string? Priority, decimal? FinancialImpact, DateTime? TargetEndDate, string? Champion);
