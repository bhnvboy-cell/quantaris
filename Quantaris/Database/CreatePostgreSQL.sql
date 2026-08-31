-- Quantaris - PostgreSQL Database Creation Script
-- Run with: psql -U postgres -h localhost -d Quantaris -f CreatePostgreSQL.sql

-- Enable UUID extension (optional, for future use)
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

----------------------------------------
-- 1. Tenants
----------------------------------------
CREATE TABLE IF NOT EXISTS Tenants (
    Id SERIAL PRIMARY KEY,
    TenantCode VARCHAR(50) NOT NULL UNIQUE,
    TenantName VARCHAR(200) NOT NULL,
    Subdomain VARCHAR(100),
    IsActive BOOLEAN DEFAULT TRUE,
    IsTrial BOOLEAN DEFAULT TRUE,
    TrialEndDate TIMESTAMP,
    MaxProjects INTEGER DEFAULT 10,
    MaxUsers INTEGER DEFAULT 50,
    PlanType VARCHAR(50) DEFAULT 'Free',
    ConnectionString VARCHAR(500),
    CreatedOn TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INTEGER DEFAULT 1
);

CREATE INDEX IF NOT EXISTS idx_tenants_tenantcode ON Tenants(TenantCode);

----------------------------------------
-- 2. Users
----------------------------------------
CREATE TABLE IF NOT EXISTS Users (
    Id SERIAL PRIMARY KEY,
    TenantId INTEGER NOT NULL REFERENCES Tenants(Id),
    Username VARCHAR(100) NOT NULL,
    Email VARCHAR(200) NOT NULL,
    PasswordHash VARCHAR(500) NOT NULL,
    PasswordSalt VARCHAR(500) NOT NULL,
    FirstName VARCHAR(100),
    LastName VARCHAR(100),
    Role VARCHAR(50) DEFAULT 'User',
    IsActive BOOLEAN DEFAULT TRUE,
    LastLoginOn TIMESTAMP,
    CreatedOn TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INTEGER,
    UNIQUE(TenantId, Username),
    UNIQUE(TenantId, Email)
);

CREATE INDEX IF NOT EXISTS idx_users_tenant ON Users(TenantId);
CREATE INDEX IF NOT EXISTS idx_users_tenant_role ON Users(TenantId, Role);

----------------------------------------
-- 3. AuditLogs
----------------------------------------
CREATE TABLE IF NOT EXISTS AuditLogs (
    Id SERIAL PRIMARY KEY,
    TenantId INTEGER NOT NULL REFERENCES Tenants(Id),
    UserId INTEGER,
    Action VARCHAR(200),
    EntityType VARCHAR(100),
    EntityId INTEGER,
    Description TEXT,
    IpAddress VARCHAR(50),
    CreatedOn TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_auditlogs_tenant_created ON AuditLogs(TenantId, CreatedOn);

----------------------------------------
-- 4. QuantarisProjects
----------------------------------------
CREATE TABLE IF NOT EXISTS QuantarisProjects (
    Id SERIAL PRIMARY KEY,
    TenantId INTEGER NOT NULL REFERENCES Tenants(Id),
    ProjectCode VARCHAR(50) NOT NULL,
    ProjectName VARCHAR(200) NOT NULL,
    Description TEXT,
    Status VARCHAR(50) DEFAULT 'NotStarted',
    Phase VARCHAR(50) DEFAULT 'Define',
    Priority VARCHAR(50),
    FinancialImpact NUMERIC(18,2),
    StartDate TIMESTAMP,
    TargetEndDate TIMESTAMP,
    ActualEndDate TIMESTAMP,
    ProjectOwnerId INTEGER REFERENCES Users(Id),
    Champion VARCHAR(200),
    CreatedOn TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INTEGER,
    UNIQUE(TenantId, ProjectCode)
);

CREATE INDEX IF NOT EXISTS idx_projects_tenant ON QuantarisProjects(TenantId);
CREATE INDEX IF NOT EXISTS idx_projects_tenant_status ON QuantarisProjects(TenantId, Status);
CREATE INDEX IF NOT EXISTS idx_projects_tenant_phase ON QuantarisProjects(TenantId, Phase);

----------------------------------------
-- 5. DMAICSteps
----------------------------------------
CREATE TABLE IF NOT EXISTS DMAICSteps (
    Id SERIAL PRIMARY KEY,
    TenantId INTEGER NOT NULL REFERENCES Tenants(Id),
    ProjectId INTEGER NOT NULL REFERENCES QuantarisProjects(Id),
    Phase VARCHAR(50),
    StepNumber INTEGER,
    StepName VARCHAR(200),
    Status VARCHAR(50) DEFAULT 'NotStarted',
    OwnerId INTEGER REFERENCES Users(Id),
    TargetDate TIMESTAMP,
    CompletedOn TIMESTAMP,
    CreatedOn TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INTEGER,
    UNIQUE(ProjectId, StepNumber)
);

CREATE INDEX IF NOT EXISTS idx_dmaicsteps_project_step ON DMAICSteps(ProjectId, StepNumber);

----------------------------------------
-- 6. VOC_Records
----------------------------------------
CREATE TABLE IF NOT EXISTS VOC_Records (
    Id SERIAL PRIMARY KEY,
    TenantId INTEGER NOT NULL REFERENCES Tenants(Id),
    ProjectId INTEGER NOT NULL REFERENCES QuantarisProjects(Id),
    CustomerName VARCHAR(200),
    VoiceStatement TEXT,
    AffinityGrouping VARCHAR(200),
    TranslatedNeed TEXT,
    Requirement TEXT,
    KanoClassification VARCHAR(50),
    CreatedOn TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INTEGER
);

CREATE INDEX IF NOT EXISTS idx_voc_tenant_project ON VOC_Records(TenantId, ProjectId);

----------------------------------------
-- 7. CTQ_Requirements
----------------------------------------
CREATE TABLE IF NOT EXISTS CTQ_Requirements (
    Id SERIAL PRIMARY KEY,
    TenantId INTEGER NOT NULL REFERENCES Tenants(Id),
    ProjectId INTEGER NOT NULL REFERENCES QuantarisProjects(Id),
    CTQName VARCHAR(200),
    Category VARCHAR(200),
    Requirement TEXT,
    Specification VARCHAR(500),
    TargetValue VARCHAR(100),
    UpperLimit VARCHAR(100),
    LowerLimit VARCHAR(100),
    IsCritical BOOLEAN DEFAULT FALSE,
    CreatedOn TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INTEGER
);

CREATE INDEX IF NOT EXISTS idx_ctq_tenant_project ON CTQ_Requirements(TenantId, ProjectId);

----------------------------------------
-- 8. ProjectCharters
----------------------------------------
CREATE TABLE IF NOT EXISTS ProjectCharters (
    Id SERIAL PRIMARY KEY,
    TenantId INTEGER NOT NULL REFERENCES Tenants(Id),
    ProjectId INTEGER NOT NULL REFERENCES QuantarisProjects(Id),
    ProblemStatement TEXT,
    GoalStatement TEXT,
    BusinessCase TEXT,
    ScopeIncluded TEXT,
    ScopeExcluded TEXT,
    Timeline TEXT,
    ElevatorSpeech TEXT,
    CreatedOn TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INTEGER
);

CREATE INDEX IF NOT EXISTS idx_charter_tenant_project ON ProjectCharters(TenantId, ProjectId);

----------------------------------------
-- 9. Stakeholders
----------------------------------------
CREATE TABLE IF NOT EXISTS Stakeholders (
    Id SERIAL PRIMARY KEY,
    TenantId INTEGER NOT NULL REFERENCES Tenants(Id),
    ProjectId INTEGER NOT NULL REFERENCES QuantarisProjects(Id),
    Name VARCHAR(200),
    Role VARCHAR(200),
    Interest VARCHAR(50),
    Influence VARCHAR(50),
    ARMI_Type VARCHAR(10),
    RACI_Type VARCHAR(10),
    Notes TEXT,
    CreatedOn TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INTEGER
);

CREATE INDEX IF NOT EXISTS idx_stakeholders_tenant_project ON Stakeholders(TenantId, ProjectId);

----------------------------------------
-- 10. SIPOC_Maps
----------------------------------------
CREATE TABLE IF NOT EXISTS SIPOC_Maps (
    Id SERIAL PRIMARY KEY,
    TenantId INTEGER NOT NULL REFERENCES Tenants(Id),
    ProjectId INTEGER NOT NULL REFERENCES QuantarisProjects(Id),
    Suppliers TEXT,
    Inputs TEXT,
    ProcessSteps TEXT,
    Outputs TEXT,
    Customers TEXT,
    CreatedOn TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INTEGER
);

CREATE INDEX IF NOT EXISTS idx_sipoc_tenant_project ON SIPOC_Maps(TenantId, ProjectId);

----------------------------------------
-- 11. MeasurementPlans
----------------------------------------
CREATE TABLE IF NOT EXISTS MeasurementPlans (
    Id SERIAL PRIMARY KEY,
    TenantId INTEGER NOT NULL REFERENCES Tenants(Id),
    ProjectId INTEGER NOT NULL REFERENCES QuantarisProjects(Id),
    MetricName VARCHAR(200),
    OperationalDefinition TEXT,
    DataSource VARCHAR(200),
    SamplingPlan TEXT,
    SampleSize INTEGER,
    Frequency VARCHAR(100),
    Who VARCHAR(200),
    MSA_Type VARCHAR(100),
    MSA_Result TEXT,
    MSA_Acceptable BOOLEAN,
    CreatedOn TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INTEGER
);

CREATE INDEX IF NOT EXISTS idx_measureplan_tenant_project ON MeasurementPlans(TenantId, ProjectId);

----------------------------------------
-- 12. Metrics
----------------------------------------
CREATE TABLE IF NOT EXISTS Metrics (
    Id SERIAL PRIMARY KEY,
    TenantId INTEGER NOT NULL REFERENCES Tenants(Id),
    ProjectId INTEGER NOT NULL REFERENCES QuantarisProjects(Id),
    MeasurementPlanId INTEGER REFERENCES MeasurementPlans(Id),
    MetricName VARCHAR(200),
    DataType VARCHAR(50),
    Baseline NUMERIC(18,4),
    Target NUMERIC(18,4),
    CurrentValue NUMERIC(18,4),
    SigmaLevel NUMERIC(18,4),
    DPMO NUMERIC(18,4),
    CapabilityCp NUMERIC(18,4),
    CapabilityCpk NUMERIC(18,4),
    NormalityTestResult VARCHAR(50),
    CreatedOn TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INTEGER
);

CREATE INDEX IF NOT EXISTS idx_metrics_tenant_project ON Metrics(TenantId, ProjectId);

----------------------------------------
-- 13. RootCauseAnalysis
----------------------------------------
CREATE TABLE IF NOT EXISTS RootCauseAnalysis (
    Id SERIAL PRIMARY KEY,
    TenantId INTEGER NOT NULL REFERENCES Tenants(Id),
    ProjectId INTEGER NOT NULL REFERENCES QuantarisProjects(Id),
    Category VARCHAR(100),
    Cause TEXT,
    Effect TEXT,
    IsFishbone BOOLEAN DEFAULT FALSE,
    FiveWhy_Chain TEXT,
    Sequence INTEGER,
    CreatedOn TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INTEGER
);

CREATE INDEX IF NOT EXISTS idx_rca_tenant_project ON RootCauseAnalysis(TenantId, ProjectId);

----------------------------------------
-- 14. FMEA_Records
----------------------------------------
CREATE TABLE IF NOT EXISTS FMEA_Records (
    Id SERIAL PRIMARY KEY,
    TenantId INTEGER NOT NULL REFERENCES Tenants(Id),
    ProjectId INTEGER NOT NULL REFERENCES QuantarisProjects(Id),
    ProcessStep VARCHAR(200),
    FailureMode TEXT,
    EffectOfFailure TEXT,
    CauseOfFailure TEXT,
    CurrentControl TEXT,
    Severity INTEGER,
    Occurrence INTEGER,
    Detection INTEGER,
    RPN INTEGER GENERATED ALWAYS AS (Severity * Occurrence * Detection) STORED,
    RecommendedAction TEXT,
    ActionTaken TEXT,
    NewSeverity INTEGER,
    NewOccurrence INTEGER,
    NewDetection INTEGER,
    NewRPN INTEGER GENERATED ALWAYS AS (COALESCE(NewSeverity, Severity) * COALESCE(NewOccurrence, Occurrence) * COALESCE(NewDetection, Detection)) STORED,
    CreatedOn TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INTEGER
);

CREATE INDEX IF NOT EXISTS idx_fmea_tenant_project ON FMEA_Records(TenantId, ProjectId);

----------------------------------------
-- 15. SolutionIdeas
----------------------------------------
CREATE TABLE IF NOT EXISTS SolutionIdeas (
    Id SERIAL PRIMARY KEY,
    TenantId INTEGER NOT NULL REFERENCES Tenants(Id),
    ProjectId INTEGER NOT NULL REFERENCES QuantarisProjects(Id),
    IdeaTitle VARCHAR(200),
    Description TEXT,
    Technique VARCHAR(100),
    LeanToolType VARCHAR(100),
    Category VARCHAR(100),
    SelectionScore NUMERIC(18,2),
    SelectionCriteria TEXT,
    IsSelected BOOLEAN DEFAULT FALSE,
    CreatedOn TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INTEGER
);

CREATE INDEX IF NOT EXISTS idx_solutions_tenant_project ON SolutionIdeas(TenantId, ProjectId);

----------------------------------------
-- 16. PilotPlans
----------------------------------------
CREATE TABLE IF NOT EXISTS PilotPlans (
    Id SERIAL PRIMARY KEY,
    TenantId INTEGER NOT NULL REFERENCES Tenants(Id),
    ProjectId INTEGER NOT NULL REFERENCES QuantarisProjects(Id),
    PlanName VARCHAR(200),
    Scope TEXT,
    Duration VARCHAR(100),
    StartDate TIMESTAMP,
    EndDate TIMESTAMP,
    Metrics TEXT,
    PokaYokeIdeas TEXT,
    ImplementationPlan TEXT,
    Status VARCHAR(50),
    CreatedOn TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INTEGER
);

CREATE INDEX IF NOT EXISTS idx_pilot_tenant_project ON PilotPlans(TenantId, ProjectId);

----------------------------------------
-- 17. ControlPlans
----------------------------------------
CREATE TABLE IF NOT EXISTS ControlPlans (
    Id SERIAL PRIMARY KEY,
    TenantId INTEGER NOT NULL REFERENCES Tenants(Id),
    ProjectId INTEGER NOT NULL REFERENCES QuantarisProjects(Id),
    ProcessStep VARCHAR(200),
    ControlMethod TEXT,
    Specification VARCHAR(500),
    SampleSize INTEGER,
    Frequency VARCHAR(100),
    ReactionPlan TEXT,
    Responsible VARCHAR(200),
    CreatedOn TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INTEGER
);

CREATE INDEX IF NOT EXISTS idx_controlplan_tenant_project ON ControlPlans(TenantId, ProjectId);

----------------------------------------
-- 18. BestPractices
----------------------------------------
CREATE TABLE IF NOT EXISTS BestPractices (
    Id SERIAL PRIMARY KEY,
    TenantId INTEGER NOT NULL REFERENCES Tenants(Id),
    ProjectId INTEGER NOT NULL REFERENCES QuantarisProjects(Id),
    Title VARCHAR(200),
    Description TEXT,
    ApplicableAreas TEXT,
    Sites TEXT,
    Teams TEXT,
    IsReplicated BOOLEAN DEFAULT FALSE,
    ReplicatedOn TIMESTAMP,
    CreatedOn TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INTEGER
);

CREATE INDEX IF NOT EXISTS idx_bestpractices_tenant_project ON BestPractices(TenantId, ProjectId);

----------------------------------------
-- 19. Celebrations
----------------------------------------
CREATE TABLE IF NOT EXISTS Celebrations (
    Id SERIAL PRIMARY KEY,
    TenantId INTEGER NOT NULL REFERENCES Tenants(Id),
    ProjectId INTEGER NOT NULL REFERENCES QuantarisProjects(Id),
    CelebrationType VARCHAR(100),
    Title VARCHAR(200),
    Description TEXT,
    Participants TEXT,
    RecognitionDate TIMESTAMP,
    CreatedOn TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INTEGER
);

CREATE INDEX IF NOT EXISTS idx_celebrations_tenant_project ON Celebrations(TenantId, ProjectId);

----------------------------------------
-- 20. MetricsHistory
----------------------------------------
CREATE TABLE IF NOT EXISTS MetricsHistory (
    Id SERIAL PRIMARY KEY,
    TenantId INTEGER NOT NULL REFERENCES Tenants(Id),
    ProjectId INTEGER NOT NULL REFERENCES QuantarisProjects(Id),
    MetricId INTEGER REFERENCES Metrics(Id),
    MetricName VARCHAR(200),
    Value NUMERIC(18,4),
    RecordedOn TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Notes TEXT,
    CreatedOn TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INTEGER
);

CREATE INDEX IF NOT EXISTS idx_metricshistory_metric ON MetricsHistory(MetricId, RecordedOn);

----------------------------------------
-- SEED DATA
----------------------------------------
INSERT INTO Tenants (TenantCode, TenantName, Subdomain, IsActive, IsTrial, MaxProjects, MaxUsers, PlanType, CreatedByUserId)
VALUES ('DEMO', 'Demo Company', 'demo', TRUE, TRUE, 10, 50, 'Enterprise', 1)
ON CONFLICT (TenantCode) DO NOTHING;
