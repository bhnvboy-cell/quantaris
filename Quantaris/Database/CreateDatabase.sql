-- ============================================================
-- Quantaris - Multi-Tenant Quantaris Database
-- SQL Server Database Creation Script
-- ============================================================

-- ============================================================
-- CLEAN REBUILD - Drop all tables in dependency order
-- ============================================================
DROP TABLE IF EXISTS [dbo].[MetricsHistory]
DROP TABLE IF EXISTS [dbo].[Celebrations]
DROP TABLE IF EXISTS [dbo].[BestPractices]
DROP TABLE IF EXISTS [dbo].[ControlPlans]
DROP TABLE IF EXISTS [dbo].[PilotPlans]
DROP TABLE IF EXISTS [dbo].[SolutionIdeas]
DROP TABLE IF EXISTS [dbo].[FMEA_Records]
DROP TABLE IF EXISTS [dbo].[RootCauseAnalysis]
DROP TABLE IF EXISTS [dbo].[Metrics]
DROP TABLE IF EXISTS [dbo].[MeasurementPlans]
DROP TABLE IF EXISTS [dbo].[SIPOC_Maps]
DROP TABLE IF EXISTS [dbo].[Stakeholders]
DROP TABLE IF EXISTS [dbo].[ProjectCharters]
DROP TABLE IF EXISTS [dbo].[CTQ_Requirements]
DROP TABLE IF EXISTS [dbo].[VOC_Records]
DROP TABLE IF EXISTS [dbo].[DMAICSteps]
DROP TABLE IF EXISTS [dbo].[QuantarisProjects]
DROP TABLE IF EXISTS [dbo].[AuditLogs]
DROP TABLE IF EXISTS [dbo].[Users]
DROP TABLE IF EXISTS [dbo].[Tenants]

-- ============================================================
-- 1. TENANTS
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tenants]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Tenants] (
        [Id]               INT              IDENTITY(1,1) NOT NULL,
        [TenantCode]       NVARCHAR(50)     NOT NULL,
        [TenantName]       NVARCHAR(200)    NOT NULL,
        [Subdomain]        NVARCHAR(100)    NULL,
        [IsActive]         BIT              NOT NULL DEFAULT 1,
        [IsTrial]          BIT              NOT NULL DEFAULT 1,
        [TrialEndDate]     DATETIME         NULL,
        [MaxProjects]      INT              NOT NULL DEFAULT 10,
        [MaxUsers]         INT              NOT NULL DEFAULT 50,
        [PlanType]         NVARCHAR(50)     NOT NULL DEFAULT 'Free',
        [CreatedByUserId]  INT              NULL,
        [CreatedOn]        DATETIME         NOT NULL DEFAULT GETUTCDATE(),
        [ConnectionString] NVARCHAR(500)    NULL,
        CONSTRAINT [PK_Tenants] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [UQ_Tenants_TenantCode] UNIQUE ([TenantCode])
    )
END

-- ============================================================
-- 2. USERS
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Users] (
        [Id]              INT              IDENTITY(1,1) NOT NULL,
        [TenantId]        INT              NOT NULL,
        [Username]        NVARCHAR(100)    NOT NULL,
        [Email]           NVARCHAR(200)    NOT NULL,
        [PasswordHash]    NVARCHAR(500)    NOT NULL,
        [PasswordSalt]    NVARCHAR(500)    NOT NULL,
        [FirstName]       NVARCHAR(100)    NULL,
        [LastName]        NVARCHAR(100)    NULL,
        [Role]            NVARCHAR(50)     NOT NULL DEFAULT 'User',
        [IsActive]        BIT              NOT NULL DEFAULT 1,
        [LastLoginOn]     DATETIME         NULL,
        [CreatedByUserId] INT              NULL,
        [CreatedOn]       DATETIME         NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [UQ_Users_TenantId_Username] UNIQUE ([TenantId], [Username]),
        CONSTRAINT [UQ_Users_TenantId_Email] UNIQUE ([TenantId], [Email]),
        CONSTRAINT [FK_Users_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([Id])
    )
END

-- ============================================================
-- 3. AUDIT LOGS
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AuditLogs] (
        [Id]            INT              IDENTITY(1,1) NOT NULL,
        [TenantId]      INT              NOT NULL,
        [UserId]        INT              NULL,
        [Action]        NVARCHAR(200)    NOT NULL,
        [EntityType]    NVARCHAR(100)    NOT NULL,
        [EntityId]      INT              NULL,
        [Description]   NVARCHAR(MAX)    NULL,
        [IpAddress]     NVARCHAR(50)     NULL,
        [CreatedOn]     DATETIME         NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_AuditLogs_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([Id])
    )
END

-- ============================================================
-- 4. SIX SIGMA PROJECTS
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[QuantarisProjects]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[QuantarisProjects] (
        [Id]              INT              IDENTITY(1,1) NOT NULL,
        [TenantId]        INT              NOT NULL,
        [ProjectCode]     NVARCHAR(50)     NOT NULL,
        [ProjectName]     NVARCHAR(200)    NOT NULL,
        [Description]     NVARCHAR(MAX)    NULL,
        [Status]          NVARCHAR(50)     NOT NULL DEFAULT 'NotStarted',
        [Phase]           NVARCHAR(50)     NOT NULL DEFAULT 'Define',
        [Priority]        NVARCHAR(50)     NULL,
        [FinancialImpact] DECIMAL(18,2)    NULL,
        [StartDate]       DATETIME         NULL,
        [TargetEndDate]   DATETIME         NULL,
        [ActualEndDate]   DATETIME         NULL,
        [ProjectOwnerId]  INT              NULL,
        [Champion]        NVARCHAR(200)    NULL,
        [CreatedByUserId] INT              NULL,
        [CreatedOn]       DATETIME         NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_QuantarisProjects] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [UQ_QuantarisProjects_TenantId_ProjectCode] UNIQUE ([TenantId], [ProjectCode]),
        CONSTRAINT [FK_QuantarisProjects_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([Id])
    )
END

-- ============================================================
-- 5. DMAIC STEPS
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DMAICSteps]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DMAICSteps] (
        [Id]            INT              IDENTITY(1,1) NOT NULL,
        [TenantId]      INT              NOT NULL,
        [ProjectId]     INT              NOT NULL,
        [Phase]         NVARCHAR(50)     NOT NULL,
        [StepNumber]    INT              NOT NULL,
        [StepName]      NVARCHAR(200)    NOT NULL,
        [Status]        NVARCHAR(50)     NOT NULL DEFAULT 'NotStarted',
        [OwnerId]       INT              NULL,
        [TargetDate]    DATETIME         NULL,
        [CompletedOn]   DATETIME         NULL,
        [CreatedByUserId] INT            NULL,
        [CreatedOn]     DATETIME         NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_DMAICSteps] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [UQ_DMAICSteps_ProjectId_StepNumber] UNIQUE ([ProjectId], [StepNumber]),
        CONSTRAINT [FK_DMAICSteps_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([Id]),
        CONSTRAINT [FK_DMAICSteps_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[QuantarisProjects]([Id]),
        CONSTRAINT [FK_DMAICSteps_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [dbo].[Users]([Id])
    )
END

-- ============================================================
-- 6. VOC RECORDS
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[VOC_Records]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[VOC_Records] (
        [Id]               INT              IDENTITY(1,1) NOT NULL,
        [TenantId]         INT              NOT NULL,
        [ProjectId]        INT              NOT NULL,
        [CustomerName]     NVARCHAR(200)    NULL,
        [VoiceStatement]   NVARCHAR(MAX)    NOT NULL,
        [AffinityGrouping] NVARCHAR(200)    NULL,
        [TranslatedNeed]   NVARCHAR(MAX)    NULL,
        [Requirement]      NVARCHAR(MAX)    NULL,
        [KanoClassification] NVARCHAR(50)   NULL,
        [CreatedByUserId]  INT              NULL,
        [CreatedOn]        DATETIME         NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_VOC_Records] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_VOC_Records_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([Id]),
        CONSTRAINT [FK_VOC_Records_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[QuantarisProjects]([Id])
    )
END

-- ============================================================
-- 7. CTQ REQUIREMENTS
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CTQ_Requirements]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[CTQ_Requirements] (
        [Id]            INT              IDENTITY(1,1) NOT NULL,
        [TenantId]      INT              NOT NULL,
        [ProjectId]     INT              NOT NULL,
        [CTQName]       NVARCHAR(200)    NOT NULL,
        [Category]      NVARCHAR(200)    NULL,
        [Requirement]   NVARCHAR(MAX)    NULL,
        [Specification] NVARCHAR(MAX)    NULL,
        [TargetValue]   NVARCHAR(100)    NULL,
        [UpperLimit]    NVARCHAR(100)    NULL,
        [LowerLimit]    NVARCHAR(100)    NULL,
        [IsCritical]    BIT              NOT NULL DEFAULT 0,
        [CreatedByUserId] INT            NULL,
        [CreatedOn]     DATETIME         NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_CTQ_Requirements] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_CTQ_Requirements_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([Id]),
        CONSTRAINT [FK_CTQ_Requirements_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[QuantarisProjects]([Id])
    )
END

-- ============================================================
-- 8. PROJECT CHARTERS
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProjectCharters]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ProjectCharters] (
        [Id]              INT              IDENTITY(1,1) NOT NULL,
        [TenantId]        INT              NOT NULL,
        [ProjectId]       INT              NOT NULL,
        [ProblemStatement] NVARCHAR(MAX)   NULL,
        [GoalStatement]   NVARCHAR(MAX)    NULL,
        [BusinessCase]    NVARCHAR(MAX)    NULL,
        [ScopeIncluded]   NVARCHAR(MAX)    NULL,
        [ScopeExcluded]   NVARCHAR(MAX)    NULL,
        [Timeline]        NVARCHAR(MAX)    NULL,
        [ElevatorSpeech]  NVARCHAR(MAX)    NULL,
        [CreatedByUserId] INT              NULL,
        [CreatedOn]       DATETIME         NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_ProjectCharters] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_ProjectCharters_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([Id]),
        CONSTRAINT [FK_ProjectCharters_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[QuantarisProjects]([Id])
    )
END

-- ============================================================
-- 9. STAKEHOLDERS
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Stakeholders]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Stakeholders] (
        [Id]            INT              IDENTITY(1,1) NOT NULL,
        [TenantId]      INT              NOT NULL,
        [ProjectId]     INT              NOT NULL,
        [Name]          NVARCHAR(200)    NOT NULL,
        [Role]          NVARCHAR(200)    NULL,
        [Interest]      NVARCHAR(50)     NULL,
        [Influence]     NVARCHAR(50)     NULL,
        [ARMI_Type]     NVARCHAR(10)     NULL,
        [RACI_Type]     NVARCHAR(10)     NULL,
        [Notes]         NVARCHAR(MAX)    NULL,
        [CreatedByUserId] INT            NULL,
        [CreatedOn]     DATETIME         NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_Stakeholders] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_Stakeholders_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([Id]),
        CONSTRAINT [FK_Stakeholders_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[QuantarisProjects]([Id])
    )
END

-- ============================================================
-- 10. SIPOC MAPS
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SIPOC_Maps]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SIPOC_Maps] (
        [Id]            INT              IDENTITY(1,1) NOT NULL,
        [TenantId]      INT              NOT NULL,
        [ProjectId]     INT              NOT NULL,
        [Suppliers]     NVARCHAR(MAX)    NULL,
        [Inputs]        NVARCHAR(MAX)    NULL,
        [ProcessSteps]  NVARCHAR(MAX)    NULL,
        [Outputs]       NVARCHAR(MAX)    NULL,
        [Customers]     NVARCHAR(MAX)    NULL,
        [CreatedByUserId] INT            NULL,
        [CreatedOn]     DATETIME         NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_SIPOC_Maps] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_SIPOC_Maps_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([Id]),
        CONSTRAINT [FK_SIPOC_Maps_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[QuantarisProjects]([Id])
    )
END

-- ============================================================
-- 11. MEASUREMENT PLANS
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MeasurementPlans]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[MeasurementPlans] (
        [Id]                  INT              IDENTITY(1,1) NOT NULL,
        [TenantId]            INT              NOT NULL,
        [ProjectId]           INT              NOT NULL,
        [MetricName]          NVARCHAR(200)    NOT NULL,
        [OperationalDefinition] NVARCHAR(MAX)  NULL,
        [DataSource]          NVARCHAR(200)    NULL,
        [SamplingPlan]        NVARCHAR(MAX)    NULL,
        [SampleSize]          INT              NULL,
        [Frequency]           NVARCHAR(100)    NULL,
        [Who]                 NVARCHAR(200)    NULL,
        [MSA_Type]            NVARCHAR(100)    NULL,
        [MSA_Result]          NVARCHAR(MAX)    NULL,
        [MSA_Acceptable]      BIT              NULL,
        [CreatedByUserId]     INT              NULL,
        [CreatedOn]           DATETIME         NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_MeasurementPlans] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_MeasurementPlans_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([Id]),
        CONSTRAINT [FK_MeasurementPlans_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[QuantarisProjects]([Id])
    )
END

-- ============================================================
-- 12. METRICS
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Metrics]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Metrics] (
        [Id]                  INT              IDENTITY(1,1) NOT NULL,
        [TenantId]            INT              NOT NULL,
        [ProjectId]           INT              NOT NULL,
        [MeasurementPlanId]   INT              NULL,
        [MetricName]          NVARCHAR(200)    NOT NULL,
        [DataType]            NVARCHAR(50)     NOT NULL,
        [Baseline]            DECIMAL(18,4)    NULL,
        [Target]              DECIMAL(18,4)    NULL,
        [CurrentValue]        DECIMAL(18,4)    NULL,
        [SigmaLevel]          DECIMAL(18,4)    NULL,
        [DPMO]                DECIMAL(18,4)    NULL,
        [CapabilityCp]        DECIMAL(18,4)    NULL,
        [CapabilityCpk]       DECIMAL(18,4)    NULL,
        [NormalityTestResult] NVARCHAR(50)     NULL,
        [CreatedByUserId]     INT              NULL,
        [CreatedOn]           DATETIME         NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_Metrics] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_Metrics_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([Id]),
        CONSTRAINT [FK_Metrics_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[QuantarisProjects]([Id]),
        CONSTRAINT [FK_Metrics_MeasurementPlanId] FOREIGN KEY ([MeasurementPlanId]) REFERENCES [dbo].[MeasurementPlans]([Id])
    )
END

-- ============================================================
-- 13. ROOT CAUSE ANALYSIS
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RootCauseAnalysis]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RootCauseAnalysis] (
        [Id]            INT              IDENTITY(1,1) NOT NULL,
        [TenantId]      INT              NOT NULL,
        [ProjectId]     INT              NOT NULL,
        [Category]      NVARCHAR(100)    NULL,
        [Cause]         NVARCHAR(MAX)    NOT NULL,
        [Effect]        NVARCHAR(MAX)    NULL,
        [IsFishbone]    BIT              NOT NULL DEFAULT 0,
        [FiveWhy_Chain] NVARCHAR(MAX)    NULL,
        [Sequence]      INT              NULL,
        [CreatedByUserId] INT            NULL,
        [CreatedOn]     DATETIME         NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_RootCauseAnalysis] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_RootCauseAnalysis_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([Id]),
        CONSTRAINT [FK_RootCauseAnalysis_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[QuantarisProjects]([Id])
    )
END

-- ============================================================
-- 14. FMEA RECORDS
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FMEA_Records]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[FMEA_Records] (
        [Id]                INT              IDENTITY(1,1) NOT NULL,
        [TenantId]          INT              NOT NULL,
        [ProjectId]         INT              NOT NULL,
        [ProcessStep]       NVARCHAR(200)    NOT NULL,
        [FailureMode]       NVARCHAR(MAX)    NOT NULL,
        [EffectOfFailure]   NVARCHAR(MAX)    NULL,
        [CauseOfFailure]    NVARCHAR(MAX)    NULL,
        [CurrentControl]    NVARCHAR(MAX)    NULL,
        [Severity]          INT              NOT NULL,
        [Occurrence]        INT              NOT NULL,
        [Detection]         INT              NOT NULL,
        [RPN]               AS ([Severity] * [Occurrence] * [Detection]) PERSISTED,
        [RecommendedAction] NVARCHAR(MAX)    NULL,
        [ActionTaken]       NVARCHAR(MAX)    NULL,
        [NewSeverity]       INT              NULL,
        [NewOccurrence]     INT              NULL,
        [NewDetection]      INT              NULL,
        [NewRPN]            AS ([NewSeverity] * [NewOccurrence] * [NewDetection]) PERSISTED,
        [CreatedByUserId]   INT              NULL,
        [CreatedOn]         DATETIME         NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_FMEA_Records] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_FMEA_Records_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([Id]),
        CONSTRAINT [FK_FMEA_Records_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[QuantarisProjects]([Id])
    )
END

-- ============================================================
-- 15. SOLUTION IDEAS
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SolutionIdeas]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SolutionIdeas] (
        [Id]              INT              IDENTITY(1,1) NOT NULL,
        [TenantId]        INT              NOT NULL,
        [ProjectId]       INT              NOT NULL,
        [IdeaTitle]       NVARCHAR(200)    NOT NULL,
        [Description]     NVARCHAR(MAX)    NULL,
        [Technique]       NVARCHAR(100)    NULL,
        [LeanToolType]    NVARCHAR(100)    NULL,
        [Category]        NVARCHAR(100)    NULL,
        [SelectionScore]  DECIMAL(18,2)    NULL,
        [SelectionCriteria] NVARCHAR(MAX)  NULL,
        [IsSelected]      BIT              NOT NULL DEFAULT 0,
        [CreatedByUserId] INT              NULL,
        [CreatedOn]       DATETIME         NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_SolutionIdeas] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_SolutionIdeas_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([Id]),
        CONSTRAINT [FK_SolutionIdeas_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[QuantarisProjects]([Id])
    )
END

-- ============================================================
-- 16. PILOT PLANS
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PilotPlans]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PilotPlans] (
        [Id]              INT              IDENTITY(1,1) NOT NULL,
        [TenantId]        INT              NOT NULL,
        [ProjectId]       INT              NOT NULL,
        [PlanName]        NVARCHAR(200)    NOT NULL,
        [Scope]           NVARCHAR(MAX)    NULL,
        [Duration]        NVARCHAR(100)    NULL,
        [StartDate]       DATETIME         NULL,
        [EndDate]         DATETIME         NULL,
        [Metrics]         NVARCHAR(MAX)    NULL,
        [PokaYokeIdeas]   NVARCHAR(MAX)    NULL,
        [ImplementationPlan] NVARCHAR(MAX) NULL,
        [Status]          NVARCHAR(50)     NOT NULL DEFAULT 'NotStarted',
        [CreatedByUserId] INT              NULL,
        [CreatedOn]       DATETIME         NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_PilotPlans] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_PilotPlans_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([Id]),
        CONSTRAINT [FK_PilotPlans_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[QuantarisProjects]([Id])
    )
END

-- ============================================================
-- 17. CONTROL PLANS
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ControlPlans]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ControlPlans] (
        [Id]              INT              IDENTITY(1,1) NOT NULL,
        [TenantId]        INT              NOT NULL,
        [ProjectId]       INT              NOT NULL,
        [ProcessStep]     NVARCHAR(200)    NOT NULL,
        [ControlMethod]   NVARCHAR(MAX)    NULL,
        [Specification]   NVARCHAR(MAX)    NULL,
        [SampleSize]      INT              NULL,
        [Frequency]       NVARCHAR(100)    NULL,
        [ReactionPlan]    NVARCHAR(MAX)    NULL,
        [Responsible]     NVARCHAR(200)    NULL,
        [CreatedByUserId] INT              NULL,
        [CreatedOn]       DATETIME         NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_ControlPlans] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_ControlPlans_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([Id]),
        CONSTRAINT [FK_ControlPlans_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[QuantarisProjects]([Id])
    )
END

-- ============================================================
-- 18. BEST PRACTICES
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BestPractices]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[BestPractices] (
        [Id]              INT              IDENTITY(1,1) NOT NULL,
        [TenantId]        INT              NOT NULL,
        [ProjectId]       INT              NOT NULL,
        [Title]           NVARCHAR(200)    NOT NULL,
        [Description]     NVARCHAR(MAX)    NULL,
        [ApplicableAreas] NVARCHAR(MAX)    NULL,
        [Sites]           NVARCHAR(MAX)    NULL,
        [Teams]           NVARCHAR(MAX)    NULL,
        [IsReplicated]    BIT              NOT NULL DEFAULT 0,
        [ReplicatedOn]    DATETIME         NULL,
        [CreatedByUserId] INT              NULL,
        [CreatedOn]       DATETIME         NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_BestPractices] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_BestPractices_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([Id]),
        CONSTRAINT [FK_BestPractices_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[QuantarisProjects]([Id])
    )
END

-- ============================================================
-- 19. CELEBRATIONS
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Celebrations]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Celebrations] (
        [Id]              INT              IDENTITY(1,1) NOT NULL,
        [TenantId]        INT              NOT NULL,
        [ProjectId]       INT              NOT NULL,
        [CelebrationType] NVARCHAR(100)    NOT NULL,
        [Title]           NVARCHAR(200)    NOT NULL,
        [Description]     NVARCHAR(MAX)    NULL,
        [Participants]    NVARCHAR(MAX)    NULL,
        [RecognitionDate] DATETIME         NULL,
        [CreatedByUserId] INT              NULL,
        [CreatedOn]       DATETIME         NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_Celebrations] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_Celebrations_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([Id]),
        CONSTRAINT [FK_Celebrations_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[QuantarisProjects]([Id])
    )
END

-- ============================================================
-- 20. METRICS HISTORY
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MetricsHistory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[MetricsHistory] (
        [Id]            INT              IDENTITY(1,1) NOT NULL,
        [TenantId]      INT              NOT NULL,
        [ProjectId]     INT              NOT NULL,
        [MetricId]      INT              NULL,
        [MetricName]    NVARCHAR(200)    NOT NULL,
        [Value]         DECIMAL(18,4)    NOT NULL,
        [RecordedOn]    DATETIME         NOT NULL,
        [Notes]         NVARCHAR(MAX)    NULL,
        [CreatedByUserId] INT            NULL,
        [CreatedOn]     DATETIME         NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_MetricsHistory] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_MetricsHistory_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([Id]),
        CONSTRAINT [FK_MetricsHistory_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[QuantarisProjects]([Id]),
        CONSTRAINT [FK_MetricsHistory_MetricId] FOREIGN KEY ([MetricId]) REFERENCES [dbo].[Metrics]([Id])
    )
END

-- ============================================================
-- INDEXES
-- ============================================================

-- Index on AuditLogs (TenantId, CreatedOn) for efficient audit trail queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_AuditLogs_TenantId_CreatedOn')
    CREATE NONCLUSTERED INDEX [IX_AuditLogs_TenantId_CreatedOn]
        ON [dbo].[AuditLogs] ([TenantId], [CreatedOn])

-- Index on DMAICSteps (ProjectId, StepNumber) for ordered step retrieval
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_DMAICSteps_ProjectId_StepNumber')
    CREATE NONCLUSTERED INDEX [IX_DMAICSteps_ProjectId_StepNumber]
        ON [dbo].[DMAICSteps] ([ProjectId], [StepNumber])

-- Index on MetricsHistory (MetricId, RecordedOn) for time-series lookups
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_MetricsHistory_MetricId_RecordedOn')
    CREATE NONCLUSTERED INDEX [IX_MetricsHistory_MetricId_RecordedOn]
        ON [dbo].[MetricsHistory] ([MetricId], [RecordedOn])

-- Index on Users (TenantId, Role) for role-based queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Users_TenantId_Role')
    CREATE NONCLUSTERED INDEX [IX_Users_TenantId_Role]
        ON [dbo].[Users] ([TenantId], [Role])

-- Index on QuantarisProjects (TenantId, Status) for project listings by status
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_QuantarisProjects_TenantId_Status')
    CREATE NONCLUSTERED INDEX [IX_QuantarisProjects_TenantId_Status]
        ON [dbo].[QuantarisProjects] ([TenantId], [Status])

-- Index on QuantarisProjects (TenantId, Phase) for project listings by phase
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_QuantarisProjects_TenantId_Phase')
    CREATE NONCLUSTERED INDEX [IX_QuantarisProjects_TenantId_Phase]
        ON [dbo].[QuantarisProjects] ([TenantId], [Phase])

-- Generic indexes on (TenantId, ProjectId) combos for multi-tenant data isolation
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_VOC_Records_TenantId_ProjectId')
    CREATE NONCLUSTERED INDEX [IX_VOC_Records_TenantId_ProjectId]
        ON [dbo].[VOC_Records] ([TenantId], [ProjectId])

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_CTQ_Requirements_TenantId_ProjectId')
    CREATE NONCLUSTERED INDEX [IX_CTQ_Requirements_TenantId_ProjectId]
        ON [dbo].[CTQ_Requirements] ([TenantId], [ProjectId])

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_ProjectCharters_TenantId_ProjectId')
    CREATE NONCLUSTERED INDEX [IX_ProjectCharters_TenantId_ProjectId]
        ON [dbo].[ProjectCharters] ([TenantId], [ProjectId])

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Stakeholders_TenantId_ProjectId')
    CREATE NONCLUSTERED INDEX [IX_Stakeholders_TenantId_ProjectId]
        ON [dbo].[Stakeholders] ([TenantId], [ProjectId])

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_SIPOC_Maps_TenantId_ProjectId')
    CREATE NONCLUSTERED INDEX [IX_SIPOC_Maps_TenantId_ProjectId]
        ON [dbo].[SIPOC_Maps] ([TenantId], [ProjectId])

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_MeasurementPlans_TenantId_ProjectId')
    CREATE NONCLUSTERED INDEX [IX_MeasurementPlans_TenantId_ProjectId]
        ON [dbo].[MeasurementPlans] ([TenantId], [ProjectId])

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Metrics_TenantId_ProjectId')
    CREATE NONCLUSTERED INDEX [IX_Metrics_TenantId_ProjectId]
        ON [dbo].[Metrics] ([TenantId], [ProjectId])

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_RootCauseAnalysis_TenantId_ProjectId')
    CREATE NONCLUSTERED INDEX [IX_RootCauseAnalysis_TenantId_ProjectId]
        ON [dbo].[RootCauseAnalysis] ([TenantId], [ProjectId])

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_FMEA_Records_TenantId_ProjectId')
    CREATE NONCLUSTERED INDEX [IX_FMEA_Records_TenantId_ProjectId]
        ON [dbo].[FMEA_Records] ([TenantId], [ProjectId])

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_SolutionIdeas_TenantId_ProjectId')
    CREATE NONCLUSTERED INDEX [IX_SolutionIdeas_TenantId_ProjectId]
        ON [dbo].[SolutionIdeas] ([TenantId], [ProjectId])

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_PilotPlans_TenantId_ProjectId')
    CREATE NONCLUSTERED INDEX [IX_PilotPlans_TenantId_ProjectId]
        ON [dbo].[PilotPlans] ([TenantId], [ProjectId])

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_ControlPlans_TenantId_ProjectId')
    CREATE NONCLUSTERED INDEX [IX_ControlPlans_TenantId_ProjectId]
        ON [dbo].[ControlPlans] ([TenantId], [ProjectId])

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_BestPractices_TenantId_ProjectId')
    CREATE NONCLUSTERED INDEX [IX_BestPractices_TenantId_ProjectId]
        ON [dbo].[BestPractices] ([TenantId], [ProjectId])

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Celebrations_TenantId_ProjectId')
    CREATE NONCLUSTERED INDEX [IX_Celebrations_TenantId_ProjectId]
        ON [dbo].[Celebrations] ([TenantId], [ProjectId])

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_MetricsHistory_TenantId_ProjectId')
    CREATE NONCLUSTERED INDEX [IX_MetricsHistory_TenantId_ProjectId]
        ON [dbo].[MetricsHistory] ([TenantId], [ProjectId])

-- ============================================================
-- SEED DATA
-- ============================================================

-- Seed tenant
IF NOT EXISTS (SELECT 1 FROM [dbo].[Tenants] WHERE [TenantCode] = 'DEMO')
BEGIN
    INSERT INTO [dbo].[Tenants] ([TenantCode], [TenantName], [Subdomain], [IsActive], [IsTrial], [MaxProjects], [MaxUsers], [PlanType], [CreatedByUserId], [CreatedOn])
    VALUES ('DEMO', 'Demo Company', 'demo', 1, 1, 10, 50, 'Free', NULL, GETUTCDATE())
END

-- Seed admin user (TenantId = 1)
IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [TenantId] = 1 AND [Username] = 'admin')
BEGIN
    INSERT INTO [dbo].[Users] ([TenantId], [Username], [Email], [PasswordHash], [PasswordSalt], [FirstName], [LastName], [Role], [IsActive], [CreatedByUserId], [CreatedOn])
    VALUES (1, 'admin', 'admin@demo.com', 'DEMO_HASH_CHANGE_ME', 'DEMO_SALT_CHANGE_ME', 'System', 'Admin', 'Owner', 1, NULL, GETUTCDATE())
END

-- ============================================================
-- END OF SCRIPT
-- ============================================================
