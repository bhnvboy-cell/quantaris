-- Quantaris - Process Capability (Cp, Cpk, Ppk) (MySQL)
-- Multi-tenant: all rows scoped by TenantId

CREATE TABLE IF NOT EXISTS ProcessCapability (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    ProjectId INT NOT NULL,
    MetricName VARCHAR(200),
    USL DECIMAL(18,4),
    LSL DECIMAL(18,4),
    Target DECIMAL(18,4),
    Mean DECIMAL(18,4),
    StdDev DECIMAL(18,4),
    Cp DECIMAL(18,4),
    Cpk DECIMAL(18,4),
    Ppk DECIMAL(18,4),
    SigmaLevel DECIMAL(18,4),
    DPMO DECIMAL(18,4),
    SampleSize INT,
    DataPoints TEXT,
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_proccap_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_proccap_project FOREIGN KEY (ProjectId) REFERENCES QuantarisProjects(Id),
    CONSTRAINT fk_proccap_user FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
) ENGINE=InnoDB;

CREATE INDEX idx_processcap_tenant_project ON ProcessCapability(TenantId, ProjectId);
