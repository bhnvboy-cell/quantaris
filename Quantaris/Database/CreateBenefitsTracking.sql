-- Quantaris - Benefits Tracking (MySQL)
CREATE TABLE IF NOT EXISTS BenefitsTracking (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    ProjectId INT NOT NULL,
    BenefitName VARCHAR(200) NOT NULL,
    BenefitType VARCHAR(30) NOT NULL,
    Category VARCHAR(30) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Currency VARCHAR(3) DEFAULT 'USD',
    StartMonth DATE NOT NULL,
    EndMonth DATE NOT NULL,
    Status VARCHAR(20) DEFAULT 'Planned',
    Notes TEXT,
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_benefits_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_benefits_project FOREIGN KEY (ProjectId) REFERENCES QuantarisProjects(Id),
    CONSTRAINT fk_benefits_user FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS BenefitsActuals (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    BenefitId INT NOT NULL,
    Month DATE NOT NULL,
    ActualAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Notes TEXT,
    CONSTRAINT fk_benefitsactuals_benefit FOREIGN KEY (BenefitId) REFERENCES BenefitsTracking(Id) ON DELETE CASCADE
) ENGINE=InnoDB;
