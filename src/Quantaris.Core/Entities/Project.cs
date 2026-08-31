using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class Project : BaseEntity, ITenantEntity
{
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "NotStarted";
    public string Phase { get; set; } = "Define";
    public string? Priority { get; set; }
    public decimal? FinancialImpact { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? TargetEndDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public int? ProjectOwnerId { get; set; }
    public string? Champion { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public User? ProjectOwner { get; set; }
    public ICollection<DmaicStep> DmaicSteps { get; set; } = new List<DmaicStep>();
    public ICollection<FmeaRecord> FmeaRecords { get; set; } = new List<FmeaRecord>();
    public ICollection<VocRecord> VocRecords { get; set; } = new List<VocRecord>();
    public ICollection<CtqRequirement> CtqRequirements { get; set; } = new List<CtqRequirement>();
    public ICollection<ProjectCharter> ProjectCharters { get; set; } = new List<ProjectCharter>();
    public ICollection<Stakeholder> Stakeholders { get; set; } = new List<Stakeholder>();
    public ICollection<SipocMap> SipocMaps { get; set; } = new List<SipocMap>();
    public ICollection<MeasurementPlan> MeasurementPlans { get; set; } = new List<MeasurementPlan>();
    public ICollection<Metric> Metrics { get; set; } = new List<Metric>();
    public ICollection<RootCauseAnalysis> RootCauseAnalyses { get; set; } = new List<RootCauseAnalysis>();
    public ICollection<SolutionIdea> SolutionIdeas { get; set; } = new List<SolutionIdea>();
    public ICollection<PilotPlan> PilotPlans { get; set; } = new List<PilotPlan>();
    public ICollection<ControlPlan> ControlPlans { get; set; } = new List<ControlPlan>();
    public ICollection<BestPractice> BestPractices { get; set; } = new List<BestPractice>();
    public ICollection<Celebration> Celebrations { get; set; } = new List<Celebration>();
    public ICollection<GanttTask> GanttTasks { get; set; } = new List<GanttTask>();
    public ICollection<GanttMilestone> GanttMilestones { get; set; } = new List<GanttMilestone>();
    public ICollection<BenefitsTracking> BenefitsTrackings { get; set; } = new List<BenefitsTracking>();
    public ICollection<CopqRecord> CopqRecords { get; set; } = new List<CopqRecord>();
    public ICollection<ControlChartModel> ControlChartModels { get; set; } = new List<ControlChartModel>();
    public ICollection<GageRnRStudy> GageRnRStudies { get; set; } = new List<GageRnRStudy>();
    public ICollection<MonteCarloModel> MonteCarloModels { get; set; } = new List<MonteCarloModel>();
    public ICollection<ProcessCapabilityRecord> ProcessCapabilityRecords { get; set; } = new List<ProcessCapabilityRecord>();
    public ICollection<QfdModel> QfdModels { get; set; } = new List<QfdModel>();
    public ICollection<RegressionModel> RegressionModels { get; set; } = new List<RegressionModel>();
    public ICollection<SampleSizeCalculation> SampleSizeCalculations { get; set; } = new List<SampleSizeCalculation>();
    public ICollection<DoeExperiment> DoeExperiments { get; set; } = new List<DoeExperiment>();
    public ICollection<HoshinKanriModel> HoshinKanriModels { get; set; } = new List<HoshinKanriModel>();
    public ICollection<MetricsHistory> MetricsHistories { get; set; } = new List<MetricsHistory>();
    public ICollection<WorkflowInstance> WorkflowInstances { get; set; } = new List<WorkflowInstance>();
    public ICollection<AiRootCausePrediction> AiRootCausePredictions { get; set; } = new List<AiRootCausePrediction>();
}
