using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class WorkflowDefinition : BaseEntity, ITenantEntity
{
    public string WorkflowName { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<WorkflowState> States { get; set; } = new List<WorkflowState>();
    public ICollection<WorkflowTransition> Transitions { get; set; } = new List<WorkflowTransition>();
}

public class WorkflowState
{
    public int Id { get; set; }
    public int WorkflowId { get; set; }
    public string StateName { get; set; } = string.Empty;
    public int StateOrder { get; set; }
    public bool IsInitial { get; set; }
    public bool IsFinal { get; set; }
    public string? AssignedRole { get; set; }

    public WorkflowDefinition Workflow { get; set; } = null!;
}

public class WorkflowTransition
{
    public int Id { get; set; }
    public int WorkflowId { get; set; }
    public int FromStateId { get; set; }
    public int ToStateId { get; set; }
    public string TransitionName { get; set; } = string.Empty;
    public string? RequiredRole { get; set; }

    public WorkflowDefinition Workflow { get; set; } = null!;
    public WorkflowState FromState { get; set; } = null!;
    public WorkflowState ToState { get; set; } = null!;
}

public class WorkflowInstance : BaseEntity, ITenantEntity
{
    public int WorkflowId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public int CurrentStateId { get; set; }
    public int? AssignedUserId { get; set; }
    public string Status { get; set; } = "Active";

    public WorkflowDefinition Workflow { get; set; } = null!;
    public WorkflowState CurrentState { get; set; } = null!;
    public User? AssignedUser { get; set; }
    public ICollection<WorkflowApproval> Approvals { get; set; } = new List<WorkflowApproval>();
}

public class WorkflowApproval
{
    public int Id { get; set; }
    public int InstanceId { get; set; }
    public int TransitionId { get; set; }
    public int FromStateId { get; set; }
    public int ToStateId { get; set; }
    public int? ApprovedByUserId { get; set; }
    public string? Comments { get; set; }
    public string Decision { get; set; } = "Pending";
    public DateTime? DecidedOn { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public WorkflowInstance Instance { get; set; } = null!;
    public WorkflowTransition Transition { get; set; } = null!;
    public WorkflowState FromState { get; set; } = null!;
    public WorkflowState ToState { get; set; } = null!;
    public User? ApprovedByUser { get; set; }
}
