-- Quantaris - Design of Experiments (DOE) + Workflow Engine + AI Root Cause (MySQL)
-- Multi-tenant: all rows scoped by TenantId

CREATE TABLE IF NOT EXISTS DOE_Experiments (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    ProjectId INT NOT NULL,
    ExperimentName VARCHAR(200) NOT NULL,
    ResponseName VARCHAR(200) DEFAULT 'Response',
    DesignType VARCHAR(50) DEFAULT 'FullFactorial',
    FactorCount INT NOT NULL DEFAULT 2,
    Runs INT NOT NULL DEFAULT 4,
    Replicates INT NOT NULL DEFAULT 1,
    CenterPoints INT NOT NULL DEFAULT 0,
    FactorsJson TEXT,
    DesignMatrixJson TEXT,
    ResponsesJson TEXT,
    ANOVAJson TEXT,
    Conclusions TEXT,
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_doe_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_doe_project FOREIGN KEY (ProjectId) REFERENCES QuantarisProjects(Id),
    CONSTRAINT fk_doe_user FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
) ENGINE=InnoDB;

CREATE INDEX idx_doe_tenant_project ON DOE_Experiments(TenantId, ProjectId);

-- Workflow / Approval Engine
CREATE TABLE IF NOT EXISTS WorkflowDefinitions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    WorkflowName VARCHAR(200) NOT NULL,
    EntityType VARCHAR(100) NOT NULL,
    Description TEXT,
    IsActive TINYINT(1) DEFAULT 1,
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_wfd_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_wfd_user FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS WorkflowStates (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    WorkflowId INT NOT NULL,
    StateName VARCHAR(100) NOT NULL,
    StateOrder INT NOT NULL DEFAULT 0,
    IsInitial TINYINT(1) DEFAULT 0,
    IsFinal TINYINT(1) DEFAULT 0,
    AssignedRole VARCHAR(50),
    CONSTRAINT fk_wfs_workflow FOREIGN KEY (WorkflowId) REFERENCES WorkflowDefinitions(Id) ON DELETE CASCADE,
    UNIQUE KEY uq_wfs_workflow_state (WorkflowId, StateName)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS WorkflowTransitions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    WorkflowId INT NOT NULL,
    FromStateId INT NOT NULL,
    ToStateId INT NOT NULL,
    TransitionName VARCHAR(100) NOT NULL,
    RequiredRole VARCHAR(50),
    CONSTRAINT fk_wft_workflow FOREIGN KEY (WorkflowId) REFERENCES WorkflowDefinitions(Id) ON DELETE CASCADE,
    CONSTRAINT fk_wft_from FOREIGN KEY (FromStateId) REFERENCES WorkflowStates(Id) ON DELETE CASCADE,
    CONSTRAINT fk_wft_to FOREIGN KEY (ToStateId) REFERENCES WorkflowStates(Id) ON DELETE CASCADE,
    UNIQUE KEY uq_wft_trans (WorkflowId, FromStateId, TransitionName)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS WorkflowInstances (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    WorkflowId INT NOT NULL,
    EntityType VARCHAR(100) NOT NULL,
    EntityId INT NOT NULL,
    CurrentStateId INT NOT NULL,
    AssignedUserId INT,
    Status VARCHAR(50) DEFAULT 'Active',
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_wfi_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_wfi_workflow FOREIGN KEY (WorkflowId) REFERENCES WorkflowDefinitions(Id),
    CONSTRAINT fk_wfi_state FOREIGN KEY (CurrentStateId) REFERENCES WorkflowStates(Id),
    CONSTRAINT fk_wfi_user FOREIGN KEY (AssignedUserId) REFERENCES Users(Id)
) ENGINE=InnoDB;

CREATE INDEX idx_wf_instances_tenant ON WorkflowInstances(TenantId);
CREATE INDEX idx_wf_instances_entity ON WorkflowInstances(EntityType, EntityId);

CREATE TABLE IF NOT EXISTS WorkflowApprovals (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    InstanceId INT NOT NULL,
    TransitionId INT NOT NULL,
    FromStateId INT NOT NULL,
    ToStateId INT NOT NULL,
    ApprovedByUserId INT,
    Comments TEXT,
    Decision VARCHAR(20) NOT NULL DEFAULT 'Pending',
    DecidedOn DATETIME,
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_wfa_instance FOREIGN KEY (InstanceId) REFERENCES WorkflowInstances(Id) ON DELETE CASCADE,
    CONSTRAINT fk_wfa_transition FOREIGN KEY (TransitionId) REFERENCES WorkflowTransitions(Id),
    CONSTRAINT fk_wfa_from FOREIGN KEY (FromStateId) REFERENCES WorkflowStates(Id),
    CONSTRAINT fk_wfa_to FOREIGN KEY (ToStateId) REFERENCES WorkflowStates(Id),
    CONSTRAINT fk_wfa_user FOREIGN KEY (ApprovedByUserId) REFERENCES Users(Id)
) ENGINE=InnoDB;

CREATE INDEX idx_wf_approvals_instance ON WorkflowApprovals(InstanceId);

-- AI Root Cause Analysis
CREATE TABLE IF NOT EXISTS AI_RootCauseModels (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    ModelName VARCHAR(200) NOT NULL,
    ModelType VARCHAR(50) DEFAULT 'Bayesian',
    IsActive TINYINT(1) DEFAULT 1,
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_aircm_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_aircm_user FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS AI_RootCausePatterns (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ModelId INT NOT NULL,
    SymptomKeyword VARCHAR(200) NOT NULL,
    RootCause VARCHAR(500) NOT NULL,
    Probability DECIMAL(5,4) NOT NULL DEFAULT 0.5,
    OccurrenceCount INT NOT NULL DEFAULT 1,
    Category VARCHAR(100),
    CONSTRAINT fk_aircp_model FOREIGN KEY (ModelId) REFERENCES AI_RootCauseModels(Id) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE INDEX idx_ai_patterns_model ON AI_RootCausePatterns(ModelId);

CREATE TABLE IF NOT EXISTS AI_RootCausePredictions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    ProjectId INT NOT NULL,
    Symptoms TEXT NOT NULL,
    PredictedRootCause VARCHAR(500),
    Confidence DECIMAL(5,4),
    ModelId INT,
    WasAccurate TINYINT(1),
    ActualRootCause VARCHAR(500),
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_aircpd_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_aircpd_project FOREIGN KEY (ProjectId) REFERENCES QuantarisProjects(Id),
    CONSTRAINT fk_aircpd_model FOREIGN KEY (ModelId) REFERENCES AI_RootCauseModels(Id),
    CONSTRAINT fk_aircpd_user FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
) ENGINE=InnoDB;

CREATE INDEX idx_ai_predictions_tenant ON AI_RootCausePredictions(TenantId, ProjectId);
