-- Quantaris - Sample Size Calculator (MySQL)
CREATE TABLE IF NOT EXISTS SampleSizeCalculations (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    ProjectId INT NOT NULL,
    CalcName VARCHAR(200) NOT NULL,
    CalcType VARCHAR(30) NOT NULL,
    Parameters TEXT NOT NULL,
    Result INT NOT NULL,
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_ss_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_ss_project FOREIGN KEY (ProjectId) REFERENCES QuantarisProjects(Id),
    CONSTRAINT fk_ss_user FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
) ENGINE=InnoDB;