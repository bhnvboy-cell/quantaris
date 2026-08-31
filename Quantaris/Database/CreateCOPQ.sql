-- Quantaris - Cost of Poor Quality (COPQ) (MySQL)
-- Multi-tenant: all rows scoped by TenantId

CREATE TABLE IF NOT EXISTS COPQ (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    ProjectId INT NOT NULL,
    EntryDate DATE NOT NULL,
    AppraisalCosts DECIMAL(18,2) NOT NULL DEFAULT 0,
    PreventionCosts DECIMAL(18,2) NOT NULL DEFAULT 0,
    InternalFailureCosts DECIMAL(18,2) NOT NULL DEFAULT 0,
    ExternalFailureCosts DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalCOPQ DECIMAL(18,2),
    Revenue DECIMAL(18,2),
    Notes TEXT,
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_copq_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_copq_project FOREIGN KEY (ProjectId) REFERENCES QuantarisProjects(Id),
    CONSTRAINT fk_copq_user FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
) ENGINE=InnoDB;

CREATE INDEX idx_copq_tenant_project ON COPQ(TenantId, ProjectId);
CREATE INDEX idx_copq_entry_date ON COPQ(ProjectId, EntryDate);
