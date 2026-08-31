using Microsoft.EntityFrameworkCore;
using Quantaris.Core.Entities;
using Quantaris.Core.Entities.Common;

namespace Quantaris.Infrastructure.Data;

public class QuantarisDbContext : DbContext
{
    private static int _currentTenantId;

    public QuantarisDbContext(DbContextOptions<QuantarisDbContext> options) : base(options) { }

    public static void SetTenantId(int tenantId) => _currentTenantId = tenantId;
    public static int GetCurrentTenantId() => _currentTenantId;

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<DmaicStep> DmaicSteps => Set<DmaicStep>();
    public DbSet<VocRecord> VocRecords => Set<VocRecord>();
    public DbSet<CtqRequirement> CtqRequirements => Set<CtqRequirement>();
    public DbSet<ProjectCharter> ProjectCharters => Set<ProjectCharter>();
    public DbSet<Stakeholder> Stakeholders => Set<Stakeholder>();
    public DbSet<SipocMap> SipocMaps => Set<SipocMap>();
    public DbSet<MeasurementPlan> MeasurementPlans => Set<MeasurementPlan>();
    public DbSet<Metric> Metrics => Set<Metric>();
    public DbSet<RootCauseAnalysis> RootCauseAnalyses => Set<RootCauseAnalysis>();
    public DbSet<FmeaRecord> FmeaRecords => Set<FmeaRecord>();
    public DbSet<SolutionIdea> SolutionIdeas => Set<SolutionIdea>();
    public DbSet<PilotPlan> PilotPlans => Set<PilotPlan>();
    public DbSet<ControlPlan> ControlPlans => Set<ControlPlan>();
    public DbSet<BestPractice> BestPractices => Set<BestPractice>();
    public DbSet<Celebration> Celebrations => Set<Celebration>();
    public DbSet<MetricsHistory> MetricsHistories => Set<MetricsHistory>();
    public DbSet<BenefitsTracking> BenefitsTrackings => Set<BenefitsTracking>();
    public DbSet<BenefitsActual> BenefitsActuals => Set<BenefitsActual>();
    public DbSet<ControlChartModel> ControlChartModels => Set<ControlChartModel>();
    public DbSet<ControlChartData> ControlChartData => Set<ControlChartData>();
    public DbSet<ControlChartLimits> ControlChartLimits => Set<ControlChartLimits>();
    public DbSet<CopqRecord> CopqRecords => Set<CopqRecord>();
    public DbSet<GageRnRStudy> GageRnRStudies => Set<GageRnRStudy>();
    public DbSet<GanttTask> GanttTasks => Set<GanttTask>();
    public DbSet<GanttMilestone> GanttMilestones => Set<GanttMilestone>();
    public DbSet<HoshinKanriModel> HoshinKanriModels => Set<HoshinKanriModel>();
    public DbSet<HoshinKanriItem> HoshinKanriItems => Set<HoshinKanriItem>();
    public DbSet<MonteCarloModel> MonteCarloModels => Set<MonteCarloModel>();
    public DbSet<MonteCarloInput> MonteCarloInputs => Set<MonteCarloInput>();
    public DbSet<MonteCarloResult> MonteCarloResults => Set<MonteCarloResult>();
    public DbSet<ProcessCapabilityRecord> ProcessCapabilityRecords => Set<ProcessCapabilityRecord>();
    public DbSet<QfdModel> QfdModels => Set<QfdModel>();
    public DbSet<QfdRequirement> QfdRequirements => Set<QfdRequirement>();
    public DbSet<QfdMatrix> QfdMatrices => Set<QfdMatrix>();
    public DbSet<QfdCorrelation> QfdCorrelations => Set<QfdCorrelation>();
    public DbSet<RegressionModel> RegressionModels => Set<RegressionModel>();
    public DbSet<RegressionData> RegressionData => Set<RegressionData>();
    public DbSet<SampleSizeCalculation> SampleSizeCalculations => Set<SampleSizeCalculation>();
    public DbSet<DoeExperiment> DoeExperiments => Set<DoeExperiment>();
    public DbSet<TrainingCourse> TrainingCourses => Set<TrainingCourse>();
    public DbSet<TrainingCertification> TrainingCertifications => Set<TrainingCertification>();
    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowState> WorkflowStates => Set<WorkflowState>();
    public DbSet<WorkflowTransition> WorkflowTransitions => Set<WorkflowTransition>();
    public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();
    public DbSet<WorkflowApproval> WorkflowApprovals => Set<WorkflowApproval>();
    public DbSet<AiRootCauseModel> AiRootCauseModels => Set<AiRootCauseModel>();
    public DbSet<AiRootCausePattern> AiRootCausePatterns => Set<AiRootCausePattern>();
    public DbSet<AiRootCausePrediction> AiRootCausePredictions => Set<AiRootCausePrediction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType) && entityType.ClrType != typeof(Tenant))
            {
                var method = typeof(QuantarisDbContext)
                    .GetMethod(nameof(ApplyTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(null, new object[] { modelBuilder });
            }
        }

        modelBuilder.Entity<Tenant>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.TenantCode).HasMaxLength(50).IsRequired();
            e.Property(t => t.TenantName).HasMaxLength(200).IsRequired();
            e.HasIndex(t => t.TenantCode).IsUnique();
        });

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Username).HasMaxLength(100).IsRequired();
            e.Property(u => u.Email).HasMaxLength(200).IsRequired();
            e.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
            e.Property(u => u.FirstName).HasMaxLength(100);
            e.Property(u => u.LastName).HasMaxLength(100);
            e.Property(u => u.Role).HasMaxLength(50);
            e.HasIndex(u => new { u.TenantId, u.Username }).IsUnique();
            e.HasIndex(u => new { u.TenantId, u.Email }).IsUnique();
            e.HasOne(u => u.Tenant).WithMany(t => t.Users).HasForeignKey(u => u.TenantId);
        });

        modelBuilder.Entity<Project>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.ProjectCode).HasMaxLength(50).IsRequired();
            e.Property(p => p.ProjectName).HasMaxLength(200).IsRequired();
            e.Property(p => p.Status).HasMaxLength(50);
            e.Property(p => p.Phase).HasMaxLength(50);
            e.Property(p => p.Priority).HasMaxLength(50);
            e.Property(p => p.Champion).HasMaxLength(200);
            e.HasIndex(p => new { p.TenantId, p.ProjectCode }).IsUnique();
            e.HasOne(p => p.Tenant).WithMany(t => t.Projects).HasForeignKey(p => p.TenantId);
            e.HasOne(p => p.ProjectOwner).WithMany().HasForeignKey(p => p.ProjectOwnerId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<DmaicStep>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Phase).HasMaxLength(50);
            e.Property(s => s.StepName).HasMaxLength(200);
            e.Property(s => s.Status).HasMaxLength(50);
            e.HasIndex(s => new { s.ProjectId, s.StepNumber }).IsUnique();
            e.HasOne(s => s.Project).WithMany(p => p.DmaicSteps).HasForeignKey(s => s.ProjectId);
        });

        modelBuilder.Entity<FmeaRecord>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.ProcessStep).HasMaxLength(200);
            e.HasOne(f => f.Project).WithMany(p => p.FmeaRecords).HasForeignKey(f => f.ProjectId);
        });

        modelBuilder.Entity<ControlPlan>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.ProcessStep).HasMaxLength(200);
            e.Property(c => c.Responsible).HasMaxLength(200);
            e.HasOne(c => c.Project).WithMany(p => p.ControlPlans).HasForeignKey(c => c.ProjectId);
        });

        modelBuilder.Entity<BenefitsTracking>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.BenefitName).HasMaxLength(200).IsRequired();
            e.Property(b => b.BenefitType).HasMaxLength(30).IsRequired();
            e.Property(b => b.Category).HasMaxLength(30).IsRequired();
            e.Property(b => b.Currency).HasMaxLength(3);
            e.Property(b => b.Status).HasMaxLength(20);
            e.HasOne(b => b.Project).WithMany(p => p.BenefitsTrackings).HasForeignKey(b => b.ProjectId);
        });

        modelBuilder.Entity<BenefitsActual>(e =>
        {
            e.HasKey(ba => ba.Id);
            e.HasOne(ba => ba.Benefit).WithMany(b => b.Actuals).HasForeignKey(ba => ba.BenefitId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ControlChartModel>(e =>
        {
            e.HasKey(cc => cc.Id);
            e.Property(cc => cc.ChartName).HasMaxLength(200).IsRequired();
            e.Property(cc => cc.ChartType).HasMaxLength(20).IsRequired();
            e.HasOne(cc => cc.Project).WithMany(p => p.ControlChartModels).HasForeignKey(cc => cc.ProjectId);
        });

        modelBuilder.Entity<ControlChartData>(e =>
        {
            e.HasKey(cd => cd.Id);
            e.HasOne(cd => cd.Model).WithMany(m => m.DataPoints).HasForeignKey(cd => cd.ModelId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ControlChartLimits>(e =>
        {
            e.HasKey(cl => cl.Id);
            e.HasOne(cl => cl.Model).WithOne(m => m.Limits).HasForeignKey<ControlChartLimits>(cl => cl.ModelId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GanttTask>(e =>
        {
            e.HasKey(g => g.Id);
            e.Property(g => g.TaskName).HasMaxLength(300).IsRequired();
            e.Property(g => g.Phase).HasMaxLength(50);
            e.Property(g => g.Priority).HasMaxLength(20);
            e.Property(g => g.Status).HasMaxLength(50);
            e.HasOne(g => g.Project).WithMany(p => p.GanttTasks).HasForeignKey(g => g.ProjectId);
            e.HasOne(g => g.Dependency).WithMany().HasForeignKey(g => g.DependencyId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(g => g.AssignedTo).WithMany().HasForeignKey(g => g.AssignedToUserId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<GanttMilestone>(e =>
        {
            e.HasKey(gm => gm.Id);
            e.Property(gm => gm.MilestoneName).HasMaxLength(300).IsRequired();
            e.Property(gm => gm.Phase).HasMaxLength(50);
            e.HasOne(gm => gm.Project).WithMany(p => p.GanttMilestones).HasForeignKey(gm => gm.ProjectId);
        });

        modelBuilder.Entity<CopqRecord>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasOne(c => c.Project).WithMany(p => p.CopqRecords).HasForeignKey(c => c.ProjectId);
        });

        modelBuilder.Entity<GageRnRStudy>(e =>
        {
            e.HasKey(g => g.Id);
            e.Property(g => g.StudyName).HasMaxLength(200).IsRequired();
            e.HasOne(g => g.Project).WithMany(p => p.GageRnRStudies).HasForeignKey(g => g.ProjectId);
        });

        modelBuilder.Entity<ProcessCapabilityRecord>(e =>
        {
            e.HasKey(pc => pc.Id);
            e.Property(pc => pc.MetricName).HasMaxLength(200);
            e.HasOne(pc => pc.Project).WithMany(p => p.ProcessCapabilityRecords).HasForeignKey(pc => pc.ProjectId);
        });

        modelBuilder.Entity<MonteCarloModel>(e =>
        {
            e.HasKey(mc => mc.Id);
            e.Property(mc => mc.ModelName).HasMaxLength(200).IsRequired();
            e.HasOne(mc => mc.Project).WithMany(p => p.MonteCarloModels).HasForeignKey(mc => mc.ProjectId);
        });

        modelBuilder.Entity<MonteCarloInput>(e =>
        {
            e.HasKey(mci => mci.Id);
            e.Property(mci => mci.InputName).HasMaxLength(100).IsRequired();
            e.HasOne(mci => mci.Model).WithMany(m => m.Inputs).HasForeignKey(mci => mci.ModelId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MonteCarloResult>(e =>
        {
            e.HasKey(mcr => mcr.Id);
            e.HasOne(mcr => mcr.Model).WithMany(m => m.Results).HasForeignKey(mcr => mcr.ModelId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QfdModel>(e =>
        {
            e.HasKey(q => q.Id);
            e.Property(q => q.ModelName).HasMaxLength(200).IsRequired();
            e.HasOne(q => q.Project).WithMany(p => p.QfdModels).HasForeignKey(q => q.ProjectId);
        });

        modelBuilder.Entity<QfdRequirement>(e =>
        {
            e.HasKey(qr => qr.Id);
            e.Property(qr => qr.ReqType).HasMaxLength(20).IsRequired();
            e.Property(qr => qr.ReqName).HasMaxLength(200).IsRequired();
            e.HasOne(qr => qr.Model).WithMany(m => m.Requirements).HasForeignKey(qr => qr.ModelId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QfdMatrix>(e =>
        {
            e.HasKey(qm => qm.Id);
            e.HasOne(qm => qm.Model).WithMany(m => m.Matrices).HasForeignKey(qm => qm.ModelId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(qm => qm.CustomerReq).WithMany().HasForeignKey(qm => qm.CustomerReqId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(qm => qm.TechnicalReq).WithMany().HasForeignKey(qm => qm.TechnicalReqId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QfdCorrelation>(e =>
        {
            e.HasKey(qc => qc.Id);
            e.Property(qc => qc.Correlation).HasMaxLength(10).IsRequired();
            e.HasOne(qc => qc.Model).WithMany(m => m.Correlations).HasForeignKey(qc => qc.ModelId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(qc => qc.TechReq1).WithMany().HasForeignKey(qc => qc.TechReq1Id).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(qc => qc.TechReq2).WithMany().HasForeignKey(qc => qc.TechReq2Id).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RegressionModel>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.ModelName).HasMaxLength(200).IsRequired();
            e.Property(r => r.ResponseName).HasMaxLength(100).IsRequired();
            e.HasOne(r => r.Project).WithMany(p => p.RegressionModels).HasForeignKey(r => r.ProjectId);
        });

        modelBuilder.Entity<RegressionData>(e =>
        {
            e.HasKey(rd => rd.Id);
            e.HasOne(rd => rd.Model).WithMany(m => m.DataPoints).HasForeignKey(rd => rd.ModelId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SampleSizeCalculation>(e =>
        {
            e.HasKey(ss => ss.Id);
            e.Property(ss => ss.CalcName).HasMaxLength(200).IsRequired();
            e.Property(ss => ss.CalcType).HasMaxLength(30).IsRequired();
            e.HasOne(ss => ss.Project).WithMany(p => p.SampleSizeCalculations).HasForeignKey(ss => ss.ProjectId);
        });

        modelBuilder.Entity<DoeExperiment>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.ExperimentName).HasMaxLength(200).IsRequired();
            e.Property(d => d.DesignType).HasMaxLength(50);
            e.HasOne(d => d.Project).WithMany(p => p.DoeExperiments).HasForeignKey(d => d.ProjectId);
        });

        modelBuilder.Entity<HoshinKanriModel>(e =>
        {
            e.HasKey(hk => hk.Id);
            e.Property(hk => hk.ModelName).HasMaxLength(200).IsRequired();
            e.HasOne(hk => hk.Project).WithMany(p => p.HoshinKanriModels).HasForeignKey(hk => hk.ProjectId);
        });

        modelBuilder.Entity<HoshinKanriItem>(e =>
        {
            e.HasKey(hki => hki.Id);
            e.Property(hki => hki.ItemType).HasMaxLength(20).IsRequired();
            e.Property(hki => hki.Status).HasMaxLength(20);
            e.HasOne(hki => hki.Model).WithMany(m => m.Items).HasForeignKey(hki => hki.ModelId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(hki => hki.Parent).WithMany(p => p.Children).HasForeignKey(hki => hki.ParentId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TrainingCourse>(e =>
        {
            e.HasKey(tc => tc.Id);
            e.Property(tc => tc.CourseCode).HasMaxLength(20).IsRequired();
            e.Property(tc => tc.CourseName).HasMaxLength(200).IsRequired();
            e.Property(tc => tc.BeltLevel).HasMaxLength(20);
        });

        modelBuilder.Entity<TrainingCertification>(e =>
        {
            e.HasKey(tcert => tcert.Id);
            e.Property(tcert => tcert.CertificationName).HasMaxLength(200).IsRequired();
            e.Property(tcert => tcert.BeltLevel).HasMaxLength(20).IsRequired();
            e.HasOne(tcert => tcert.User).WithMany().HasForeignKey(tcert => tcert.UserId);
        });

        modelBuilder.Entity<WorkflowDefinition>(e =>
        {
            e.HasKey(wf => wf.Id);
            e.Property(wf => wf.WorkflowName).HasMaxLength(200).IsRequired();
            e.Property(wf => wf.EntityType).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<WorkflowState>(e =>
        {
            e.HasKey(ws => ws.Id);
            e.Property(ws => ws.StateName).HasMaxLength(100).IsRequired();
            e.Property(ws => ws.AssignedRole).HasMaxLength(50);
            e.HasOne(ws => ws.Workflow).WithMany(w => w.States).HasForeignKey(ws => ws.WorkflowId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(ws => new { ws.WorkflowId, ws.StateName }).IsUnique();
        });

        modelBuilder.Entity<WorkflowTransition>(e =>
        {
            e.HasKey(wt => wt.Id);
            e.Property(wt => wt.TransitionName).HasMaxLength(100).IsRequired();
            e.Property(wt => wt.RequiredRole).HasMaxLength(50);
            e.HasOne(wt => wt.Workflow).WithMany(w => w.Transitions).HasForeignKey(wt => wt.WorkflowId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(wt => wt.FromState).WithMany().HasForeignKey(wt => wt.FromStateId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(wt => wt.ToState).WithMany().HasForeignKey(wt => wt.ToStateId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkflowInstance>(e =>
        {
            e.HasKey(wi => wi.Id);
            e.Property(wi => wi.EntityType).HasMaxLength(100).IsRequired();
            e.Property(wi => wi.Status).HasMaxLength(50);
            e.HasOne(wi => wi.Workflow).WithMany().HasForeignKey(wi => wi.WorkflowId);
            e.HasOne(wi => wi.CurrentState).WithMany().HasForeignKey(wi => wi.CurrentStateId);
            e.HasOne(wi => wi.AssignedUser).WithMany().HasForeignKey(wi => wi.AssignedUserId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<WorkflowApproval>(e =>
        {
            e.HasKey(wa => wa.Id);
            e.Property(wa => wa.Decision).HasMaxLength(20);
            e.HasOne(wa => wa.Instance).WithMany(i => i.Approvals).HasForeignKey(wa => wa.InstanceId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(wa => wa.Transition).WithMany().HasForeignKey(wa => wa.TransitionId);
            e.HasOne(wa => wa.FromState).WithMany().HasForeignKey(wa => wa.FromStateId);
            e.HasOne(wa => wa.ToState).WithMany().HasForeignKey(wa => wa.ToStateId);
            e.HasOne(wa => wa.ApprovedByUser).WithMany().HasForeignKey(wa => wa.ApprovedByUserId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AiRootCauseModel>(e =>
        {
            e.HasKey(arc => arc.Id);
            e.Property(arc => arc.ModelName).HasMaxLength(200).IsRequired();
            e.Property(arc => arc.ModelType).HasMaxLength(50);
        });

        modelBuilder.Entity<AiRootCausePattern>(e =>
        {
            e.HasKey(arcp => arcp.Id);
            e.Property(arcp => arcp.SymptomKeyword).HasMaxLength(200).IsRequired();
            e.Property(arcp => arcp.RootCause).HasMaxLength(500).IsRequired();
            e.HasOne(arcp => arcp.Model).WithMany(m => m.Patterns).HasForeignKey(arcp => arcp.ModelId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AiRootCausePrediction>(e =>
        {
            e.HasKey(arcpd => arcpd.Id);
            e.HasOne(arcpd => arcpd.Project).WithMany(p => p.AiRootCausePredictions).HasForeignKey(arcpd => arcpd.ProjectId);
            e.HasOne(arcpd => arcpd.Model).WithMany().HasForeignKey(arcpd => arcpd.ModelId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Action).HasMaxLength(200);
            e.Property(a => a.EntityType).HasMaxLength(100);
            e.Property(a => a.IpAddress).HasMaxLength(50);
            e.HasIndex(a => new { a.TenantId, a.CreatedOn });
        });

        modelBuilder.Entity<VocRecord>(e =>
        {
            e.HasKey(v => v.Id);
            e.Property(v => v.CustomerName).HasMaxLength(200);
            e.Property(v => v.KanoClassification).HasMaxLength(50);
            e.HasOne(v => v.Project).WithMany(p => p.VocRecords).HasForeignKey(v => v.ProjectId);
        });

        modelBuilder.Entity<CtqRequirement>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.CtqName).HasMaxLength(200);
            e.Property(c => c.Specification).HasMaxLength(500);
            e.HasOne(c => c.Project).WithMany(p => p.CtqRequirements).HasForeignKey(c => c.ProjectId);
        });

        modelBuilder.Entity<ProjectCharter>(e =>
        {
            e.HasKey(pc => pc.Id);
            e.HasOne(pc => pc.Project).WithMany(p => p.ProjectCharters).HasForeignKey(pc => pc.ProjectId);
        });

        modelBuilder.Entity<Stakeholder>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Name).HasMaxLength(200);
            e.HasOne(s => s.Project).WithMany(p => p.Stakeholders).HasForeignKey(s => s.ProjectId);
        });

        modelBuilder.Entity<SipocMap>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasOne(s => s.Project).WithMany(p => p.SipocMaps).HasForeignKey(s => s.ProjectId);
        });

        modelBuilder.Entity<MeasurementPlan>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.MetricName).HasMaxLength(200);
            e.HasOne(m => m.Project).WithMany(p => p.MeasurementPlans).HasForeignKey(m => m.ProjectId);
        });

        modelBuilder.Entity<Metric>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.MetricName).HasMaxLength(200);
            e.HasOne(m => m.Project).WithMany(p => p.Metrics).HasForeignKey(m => m.ProjectId);
        });

        modelBuilder.Entity<RootCauseAnalysis>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Category).HasMaxLength(100);
            e.HasOne(r => r.Project).WithMany(p => p.RootCauseAnalyses).HasForeignKey(r => r.ProjectId);
        });

        modelBuilder.Entity<SolutionIdea>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.IdeaTitle).HasMaxLength(200);
            e.HasOne(s => s.Project).WithMany(p => p.SolutionIdeas).HasForeignKey(s => s.ProjectId);
        });

        modelBuilder.Entity<PilotPlan>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.PlanName).HasMaxLength(200);
            e.HasOne(p => p.Project).WithMany(pj => pj.PilotPlans).HasForeignKey(p => p.ProjectId);
        });

        modelBuilder.Entity<BestPractice>(e =>
        {
            e.HasKey(bp => bp.Id);
            e.Property(bp => bp.Title).HasMaxLength(200);
            e.HasOne(bp => bp.Project).WithMany(p => p.BestPractices).HasForeignKey(bp => bp.ProjectId);
        });

        modelBuilder.Entity<Celebration>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Title).HasMaxLength(200);
            e.HasOne(c => c.Project).WithMany(p => p.Celebrations).HasForeignKey(c => c.ProjectId);
        });

        modelBuilder.Entity<MetricsHistory>(e =>
        {
            e.HasKey(mh => mh.Id);
            e.HasOne(mh => mh.Metric).WithMany().HasForeignKey(mh => mh.MetricId).OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ApplyTenantFilter<T>(ModelBuilder modelBuilder) where T : class, ITenantEntity
    {
        modelBuilder.Entity<T>().HasQueryFilter(
            e => EF.Property<int>(e, nameof(ITenantEntity.TenantId)) == _currentTenantId);
    }
}
