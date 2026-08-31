using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quantaris.Infrastructure.Data;
using Quantaris.Infrastructure.Services;

namespace Quantaris.Api.Controllers;

[ApiController]
[Route("api/dmaic")]
[Authorize]
public class DmaicController : ControllerBase
{
    private readonly IDmaicService _dmaicService;
    private readonly IAuditService _audit;

    public DmaicController(IDmaicService dmaicService, IAuditService audit)
    {
        _dmaicService = dmaicService;
        _audit = audit;
    }

    [HttpGet("{projectId}/steps")]
    public async Task<IActionResult> GetSteps(int projectId)
    {
        var steps = await _dmaicService.GetStepsAsync(projectId);
        return Ok(steps);
    }

    [HttpPut("{projectId}/steps/{stepNumber}")]
    public async Task<IActionResult> UpdateStep(int projectId, int stepNumber, [FromBody] UpdateStepRequest request)
    {
        var step = await _dmaicService.UpdateStepStatusAsync(projectId, stepNumber, request.Status, request.CompletedOn);
        if (step == null) return NotFound();

        await _audit.LogAsync("Update", "DMAICStep", projectId,
            $"Step {stepNumber} status changed to {request.Status}", HttpContext.Connection.RemoteIpAddress?.ToString());

        return Ok(step);
    }

    [HttpGet("{projectId}/progress")]
    public async Task<IActionResult> GetProgress(int projectId)
    {
        var progress = await _dmaicService.GetProgressSummaryAsync(projectId);
        return Ok(progress);
    }
}

public record UpdateStepRequest(string Status, DateTime? CompletedOn);
