-- Quantaris - Gage R&R (Measurement System Analysis) (MySQL)
-- Multi-tenant: all rows scoped by TenantId

CREATE TABLE IF NOT EXISTS GageRnRStudies (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    ProjectId INT NOT NULL,
    StudyName VARCHAR(200) NOT NULL,
    PartCount INT NOT NULL DEFAULT 5,
    AppraiserCount INT NOT NULL DEFAULT 2,
    TrialCount INT NOT NULL DEFAULT 2,
    PartNames TEXT,
    AppraiserNames TEXT,
    MeasurementsJson TEXT,
    ResultsJson TEXT,
    Conclusion VARCHAR(50),
    Notes TEXT,
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_gagerr_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_gagerr_project FOREIGN KEY (ProjectId) REFERENCES QuantarisProjects(Id),
    CONSTRAINT fk_gagerr_user FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
) ENGINE=InnoDB;

CREATE INDEX idx_gagerandr_tenant_project ON GageRnRStudies(TenantId, ProjectId);
