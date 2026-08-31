-- Quantaris - MySQL (XAMPP/MariaDB) Database Creation Script
-- Run with: mysql -u root < CreateMySQL.sql
-- (Create the database first: CREATE DATABASE IF NOT EXISTS quantaris; USE quantaris;)

SET NAMES utf8mb4;

----------------------------------------
-- 1. Tenants
----------------------------------------
CREATE TABLE IF NOT EXISTS Tenants (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantCode VARCHAR(50) NOT NULL UNIQUE,
    TenantName VARCHAR(200) NOT NULL,
    Subdomain VARCHAR(100),
    IsActive TINYINT(1) DEFAULT 1,
    IsTrial TINYINT(1) DEFAULT 1,
    TrialEndDate DATETIME,
    MaxProjects INT DEFAULT 10,
    MaxUsers INT DEFAULT 50,
    PlanType VARCHAR(50) DEFAULT 'Free',
    ConnectionString VARCHAR(500),
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT DEFAULT 1
) ENGINE=InnoDB;

CREATE INDEX idx_tenants_tenantcode ON Tenants(TenantCode);

----------------------------------------
-- 2. Users
----------------------------------------
CREATE TABLE IF NOT EXISTS Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    Username VARCHAR(100) NOT NULL,
    Email VARCHAR(200) NOT NULL,
    PasswordHash VARCHAR(500) NOT NULL,
    PasswordSalt VARCHAR(500) NOT NULL,
    FirstName VARCHAR(100),
    LastName VARCHAR(100),
    Role VARCHAR(50) DEFAULT 'User',
    IsActive TINYINT(1) DEFAULT 1,
    LastLoginOn DATETIME,
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_users_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    UNIQUE KEY uq_users_tenant_username (TenantId, Username),
    UNIQUE KEY uq_users_tenant_email (TenantId, Email)
) ENGINE=InnoDB;

CREATE INDEX idx_users_tenant ON Users(TenantId);
CREATE INDEX idx_users_tenant_role ON Users(TenantId, Role);

----------------------------------------
-- 3. AuditLogs
----------------------------------------
CREATE TABLE IF NOT EXISTS AuditLogs (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    UserId INT,
    Action VARCHAR(200),
    EntityType VARCHAR(100),
    EntityId INT,
    Description TEXT,
    IpAddress VARCHAR(50),
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_auditlogs_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id)
) ENGINE=InnoDB;

CREATE INDEX idx_auditlogs_tenant_created ON AuditLogs(TenantId, CreatedOn);

----------------------------------------
-- 4. QuantarisProjects
----------------------------------------
CREATE TABLE IF NOT EXISTS QuantarisProjects (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    ProjectCode VARCHAR(50) NOT NULL,
    ProjectName VARCHAR(200) NOT NULL,
    Description TEXT,
    Status VARCHAR(50) DEFAULT 'NotStarted',
    Phase VARCHAR(50) DEFAULT 'Define',
    Priority VARCHAR(50),
    FinancialImpact DECIMAL(18,2),
    StartDate DATETIME,
    TargetEndDate DATETIME,
    ActualEndDate DATETIME,
    ProjectOwnerId INT,
    Champion VARCHAR(200),
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_projects_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_projects_owner FOREIGN KEY (ProjectOwnerId) REFERENCES Users(Id),
    UNIQUE KEY uq_projects_tenant_code (TenantId, ProjectCode)
) ENGINE=InnoDB;

CREATE INDEX idx_projects_tenant ON QuantarisProjects(TenantId);
CREATE INDEX idx_projects_tenant_status ON QuantarisProjects(TenantId, Status);
CREATE INDEX idx_projects_tenant_phase ON QuantarisProjects(TenantId, Phase);

----------------------------------------
-- 5. DMAICSteps
----------------------------------------
CREATE TABLE IF NOT EXISTS DMAICSteps (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    ProjectId INT NOT NULL,
    Phase VARCHAR(50),
    StepNumber INT,
    StepName VARCHAR(200),
    Status VARCHAR(50) DEFAULT 'NotStarted',
    OwnerId INT,
    TargetDate DATETIME,
    CompletedOn DATETIME,
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_dmaicsteps_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_dmaicsteps_project FOREIGN KEY (ProjectId) REFERENCES QuantarisProjects(Id),
    UNIQUE KEY uq_dmaicsteps_project_step (ProjectId, StepNumber)
) ENGINE=InnoDB;

CREATE INDEX idx_dmaicsteps_project_step ON DMAICSteps(ProjectId, StepNumber);

----------------------------------------
-- 6. VOC_Records
----------------------------------------
CREATE TABLE IF NOT EXISTS VOC_Records (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    ProjectId INT NOT NULL,
    CustomerName VARCHAR(200),
    VoiceStatement TEXT,
    AffinityGrouping VARCHAR(200),
    TranslatedNeed TEXT,
    Requirement TEXT,
    KanoClassification VARCHAR(50),
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_voc_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_voc_project FOREIGN KEY (ProjectId) REFERENCES QuantarisProjects(Id)
) ENGINE=InnoDB;

CREATE INDEX idx_voc_tenant_project ON VOC_Records(TenantId, ProjectId);

----------------------------------------
-- 7. CTQ_Requirements
----------------------------------------
CREATE TABLE IF NOT EXISTS CTQ_Requirements (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    ProjectId INT NOT NULL,
    CTQName VARCHAR(200),
    Category VARCHAR(200),
    Requirement TEXT,
    Specification VARCHAR(500),
    TargetValue VARCHAR(100),
    UpperLimit VARCHAR(100),
    LowerLimit VARCHAR(100),
    IsCritical TINYINT(1) DEFAULT 0,
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_ctq_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_ctq_project FOREIGN KEY (ProjectId) REFERENCES QuantarisProjects(Id)
) ENGINE=InnoDB;

CREATE INDEX idx_ctq_tenant_project ON CTQ_Requirements(TenantId, ProjectId);

----------------------------------------
-- 8. ProjectCharters
----------------------------------------
CREATE TABLE IF NOT EXISTS ProjectCharters (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    ProjectId INT NOT NULL,
    ProblemStatement TEXT,
    GoalStatement TEXT,
    BusinessCase TEXT,
    ScopeIncluded TEXT,
    ScopeExcluded TEXT,
    Timeline TEXT,
    ElevatorSpeech TEXT,
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_charter_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_charter_project FOREIGN KEY (ProjectId) REFERENCES QuantarisProjects(Id)
) ENGINE=InnoDB;

CREATE INDEX idx_charter_tenant_project ON ProjectCharters(TenantId, ProjectId);

----------------------------------------
-- 9. Stakeholders
----------------------------------------
CREATE TABLE IF NOT EXISTS Stakeholders (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    ProjectId INT NOT NULL,
    Name VARCHAR(200),
    Role VARCHAR(200),
    Interest VARCHAR(50),
    Influence VARCHAR(50),
    ARMI_Type VARCHAR(10),
    RACI_Type VARCHAR(10),
    Notes TEXT,
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_stakeholders_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_stakeholders_project FOREIGN KEY (ProjectId) REFERENCES QuantarisProjects(Id)
) ENGINE=InnoDB;

CREATE INDEX idx_stakeholders_tenant_project ON Stakeholders(TenantId, ProjectId);

----------------------------------------
-- 10. SIPOC_Maps
----------------------------------------
CREATE TABLE IF NOT EXISTS SIPOC_Maps (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    ProjectId INT NOT NULL,
    Suppliers TEXT,
    Inputs TEXT,
    ProcessSteps TEXT,
    Outputs TEXT,
    Customers TEXT,
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_sipoc_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_sipoc_project FOREIGN KEY (ProjectId) REFERENCES QuantarisProjects(Id)
) ENGINE=InnoDB;

CREATE INDEX idx_sipoc_tenant_project ON SIPOC_Maps(TenantId, ProjectId);

----------------------------------------
-- 11. MeasurementPlans
----------------------------------------
CREATE TABLE IF NOT EXISTS MeasurementPlans (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    ProjectId INT NOT NULL,
    MetricName VARCHAR(200),
    OperationalDefinition TEXT,
    DataSource VARCHAR(200),
    SamplingPlan TEXT,
    SampleSize INT,
    Frequency VARCHAR(100),
    Who VARCHAR(200),
    MSA_Type VARCHAR(100),
    MSA_Result TEXT,
    MSA_Acceptable TINYINT(1),
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_measureplan_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_measureplan_project FOREIGN KEY (ProjectId) REFERENCES QuantarisProjects(Id)
) ENGINE=InnoDB;

CREATE INDEX idx_measureplan_tenant_project ON MeasurementPlans(TenantId, ProjectId);

----------------------------------------
-- 12. Metrics
----------------------------------------
CREATE TABLE IF NOT EXISTS Metrics (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    ProjectId INT NOT NULL,
    MeasurementPlanId INT,
    MetricName VARCHAR(200),
    DataType VARCHAR(50),
    Baseline DECIMAL(18,4),
    Target DECIMAL(18,4),
    CurrentValue DECIMAL(18,4),
    SigmaLevel DECIMAL(18,4),
    DPMO DECIMAL(18,4),
    CapabilityCp DECIMAL(18,4),
    CapabilityCpk DECIMAL(18,4),
    NormalityTestResult VARCHAR(50),
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_metrics_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_metrics_project FOREIGN KEY (ProjectId) REFERENCES QuantarisProjects(Id)
) ENGINE=InnoDB;

CREATE INDEX idx_metrics_tenant_project ON Metrics(TenantId, ProjectId);

----------------------------------------
-- 13. RootCauseAnalysis
----------------------------------------
CREATE TABLE IF NOT EXISTS RootCauseAnalysis (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    ProjectId INT NOT NULL,
    Category VARCHAR(100),
    Cause TEXT,
    Effect TEXT,
    IsFishbone TINYINT(1) DEFAULT 0,
    FiveWhy_Chain TEXT,
    Sequence INT,
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_rca_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_rca_project FOREIGN KEY (ProjectId) REFERENCES QuantarisProjects(Id)
) ENGINE=InnoDB;

CREATE INDEX idx_rca_tenant_project ON RootCauseAnalysis(TenantId, ProjectId);

----------------------------------------
-- 14. FMEA_Records
----------------------------------------
CREATE TABLE IF NOT EXISTS FMEA_Records (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    ProjectId INT NOT NULL,
    ProcessStep VARCHAR(200),
    FailureMode TEXT,
    EffectOfFailure TEXT,
    CauseOfFailure TEXT,
    CurrentControl TEXT,
    Severity INT,
    Occurrence INT,
    Detection INT,
    RPN INT,
    RecommendedAction TEXT,
    ActionTaken TEXT,
    NewSeverity INT,
    NewOccurrence INT,
    NewDetection INT,
    NewRPN INT,
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_fmea_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_fmea_project FOREIGN KEY (ProjectId) REFERENCES QuantarisProjects(Id)
) ENGINE=InnoDB;

CREATE INDEX idx_fmea_tenant_project ON FMEA_Records(TenantId, ProjectId);

----------------------------------------
-- 15. SolutionIdeas
----------------------------------------
CREATE TABLE IF NOT EXISTS SolutionIdeas (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    ProjectId INT NOT NULL,
    IdeaTitle VARCHAR(200),
    Description TEXT,
    Technique VARCHAR(100),
    LeanToolType VARCHAR(100),
    Category VARCHAR(100),
    SelectionScore DECIMAL(18,2),
    SelectionCriteria TEXT,
    IsSelected TINYINT(1) DEFAULT 0,
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_solutions_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_solutions_project FOREIGN KEY (ProjectId) REFERENCES QuantarisProjects(Id)
) ENGINE=InnoDB;

CREATE INDEX idx_solutions_tenant_project ON SolutionIdeas(TenantId, ProjectId);

----------------------------------------
-- 16. PilotPlans
----------------------------------------
CREATE TABLE IF NOT EXISTS PilotPlans (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    ProjectId INT NOT NULL,
    PlanName VARCHAR(200),
    Scope TEXT,
    Duration VARCHAR(100),
    StartDate DATETIME,
    EndDate DATETIME,
    Metrics TEXT,
    PokaYokeIdeas TEXT,
    ImplementationPlan TEXT,
    Status VARCHAR(50),
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_pilot_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_pilot_project FOREIGN KEY (ProjectId) REFERENCES QuantarisProjects(Id)
) ENGINE=InnoDB;

CREATE INDEX idx_pilot_tenant_project ON PilotPlans(TenantId, ProjectId);

----------------------------------------
-- 17. ControlPlans
----------------------------------------
CREATE TABLE IF NOT EXISTS ControlPlans (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    ProjectId INT NOT NULL,
    ProcessStep VARCHAR(200),
    ControlMethod TEXT,
    Specification VARCHAR(500),
    SampleSize INT,
    Frequency VARCHAR(100),
    ReactionPlan TEXT,
    Responsible VARCHAR(200),
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_controlplan_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_controlplan_project FOREIGN KEY (ProjectId) REFERENCES QuantarisProjects(Id)
) ENGINE=InnoDB;

CREATE INDEX idx_controlplan_tenant_project ON ControlPlans(TenantId, ProjectId);

----------------------------------------
-- 18. BestPractices
----------------------------------------
CREATE TABLE IF NOT EXISTS BestPractices (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    ProjectId INT NOT NULL,
    Title VARCHAR(200),
    Description TEXT,
    ApplicableAreas TEXT,
    Sites TEXT,
    Teams TEXT,
    IsReplicated TINYINT(1) DEFAULT 0,
    ReplicatedOn DATETIME,
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_bestpractices_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_bestpractices_project FOREIGN KEY (ProjectId) REFERENCES QuantarisProjects(Id)
) ENGINE=InnoDB;

CREATE INDEX idx_bestpractices_tenant_project ON BestPractices(TenantId, ProjectId);

----------------------------------------
-- 19. Celebrations
----------------------------------------
CREATE TABLE IF NOT EXISTS Celebrations (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    ProjectId INT NOT NULL,
    CelebrationType VARCHAR(100),
    Title VARCHAR(200),
    Description TEXT,
    Participants TEXT,
    RecognitionDate DATETIME,
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_celebrations_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_celebrations_project FOREIGN KEY (ProjectId) REFERENCES QuantarisProjects(Id)
) ENGINE=InnoDB;

CREATE INDEX idx_celebrations_tenant_project ON Celebrations(TenantId, ProjectId);

----------------------------------------
-- 20. MetricsHistory
----------------------------------------
CREATE TABLE IF NOT EXISTS MetricsHistory (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    ProjectId INT NOT NULL,
    MetricId INT,
    MetricName VARCHAR(200),
    Value DECIMAL(18,4),
    RecordedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    Notes TEXT,
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_metricshistory_metric FOREIGN KEY (MetricId) REFERENCES Metrics(Id)
) ENGINE=InnoDB;

CREATE INDEX idx_metricshistory_metric ON MetricsHistory(MetricId, RecordedOn);

----------------------------------------
-- SEED DATA (optional; Setup.aspx also handles this)
----------------------------------------
INSERT IGNORE INTO Tenants (TenantCode, TenantName, Subdomain, IsActive, IsTrial, MaxProjects, MaxUsers, PlanType, CreatedByUserId)
VALUES ('DEMO', 'Demo Company', 'demo', 1, 1, 10, 50, 'Enterprise', 1);
