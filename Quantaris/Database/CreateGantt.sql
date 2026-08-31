-- Quantaris - Project Gantt Chart (MySQL)
-- Multi-tenant: all rows scoped by TenantId

CREATE TABLE IF NOT EXISTS GanttTasks (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    ProjectId INT NOT NULL,
    TaskName VARCHAR(300) NOT NULL,
    Description TEXT,
    Phase VARCHAR(50) DEFAULT 'Define',
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    Duration INT GENERATED ALWAYS AS (DATEDIFF(EndDate, StartDate)) STORED,
    Progress INT NOT NULL DEFAULT 0,
    DependencyId INT,
    AssignedToUserId INT,
    Priority VARCHAR(20) DEFAULT 'Medium',
    Status VARCHAR(50) DEFAULT 'Not Started',
    SortOrder INT NOT NULL DEFAULT 0,
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_gantt_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_gantt_project FOREIGN KEY (ProjectId) REFERENCES QuantarisProjects(Id),
    CONSTRAINT fk_gantt_dep FOREIGN KEY (DependencyId) REFERENCES GanttTasks(Id),
    CONSTRAINT fk_gantt_assigned FOREIGN KEY (AssignedToUserId) REFERENCES Users(Id),
    CONSTRAINT fk_gantt_user FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id),
    CONSTRAINT chk_dates CHECK (EndDate >= StartDate),
    CONSTRAINT chk_progress CHECK (Progress >= 0 AND Progress <= 100)
) ENGINE=InnoDB;

CREATE INDEX idx_gantt_tenant_project ON GanttTasks(TenantId, ProjectId);
CREATE INDEX idx_gantt_phase ON GanttTasks(ProjectId, Phase);

-- Milestones (zero-duration markers)
CREATE TABLE IF NOT EXISTS GanttMilestones (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    ProjectId INT NOT NULL,
    MilestoneName VARCHAR(300) NOT NULL,
    MilestoneDate DATE NOT NULL,
    Phase VARCHAR(50),
    IsCompleted TINYINT(1) DEFAULT 0,
    SortOrder INT NOT NULL DEFAULT 0,
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_ganttm_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_ganttm_project FOREIGN KEY (ProjectId) REFERENCES QuantarisProjects(Id),
    CONSTRAINT fk_ganttm_user FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
) ENGINE=InnoDB;

CREATE INDEX idx_gantt_milestones ON GanttMilestones(ProjectId);
